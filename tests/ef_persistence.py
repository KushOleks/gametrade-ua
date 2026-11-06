"""Integration test against a temporary SQLite database, including a server restart.
Run after dotnet build: python3 tests/ef_persistence.py
"""
import os
import re
from pathlib import Path
import runpy
import socket
import sqlite3
import subprocess
import sys
import tempfile
import time
import urllib.request

root = Path(__file__).resolve().parents[1]
runtime = os.environ.get('DOTNET8', '/opt/homebrew/opt/dotnet@8/libexec/dotnet')
with tempfile.TemporaryDirectory(prefix='gametrade-ef-') as temp:
    database = Path(temp) / 'test.db'
    with socket.socket() as probe:
        probe.bind(('127.0.0.1', 0))
        port = probe.getsockname()[1]
    base = f'http://127.0.0.1:{port}'
    env = dict(os.environ, ConnectionStrings__Marketplace=f'Data Source={database}', ASPNETCORE_ENVIRONMENT='Development')
    logfile = open(root.parent / 'ef-test.log', 'w+')
    server = None

    def start():
        process = subprocess.Popen([runtime, str(root / 'bin/Debug/net8.0/OrderSystem.dll'), '--urls', base], cwd=root, env=env, stdout=logfile, stderr=subprocess.STDOUT)
        for _ in range(100):
            if process.poll() is not None:
                logfile.seek(0)
                raise AssertionError(logfile.read()[-10000:])
            try:
                if urllib.request.urlopen(base + '/products', timeout=1).status == 200:
                    return process
            except Exception:
                time.sleep(.1)
        process.terminate()
        raise AssertionError('Server did not become ready')

    def stop():
        global server
        if server:
            server.terminate()
            server.wait(timeout=10)
            server = None

    try:
        server = start()
        # All earlier CRUD, validation, filters and query-binding tests on an isolated DB.
        sys.path.insert(0, str(root / 'tests'))
        sys.argv = ['validation_binding.py', base]
        suite = runpy.run_path(str(root / 'tests/validation_binding.py'))
        request, token, Inputs = suite['request'], suite['token'], suite['Inputs']

        def create_record(controller, data):
            status, url, body = request(f'/{controller}/Create', dict(data, __RequestVerificationToken=token(f'/{controller}/Create')))
            assert status == 200 and '/details/' in url.lower(), body
            return int(url.rsplit('/', 1)[1])

        customer = create_record('Customers', dict(FirstName='EF Buyer', LastName='Persistence', City='Київ'))
        seller = create_record('Sellers', dict(Name='EF Seller', City='Львів', Description='Demo shop'))
        for controller, item, values in [
            ('Customers', customer, dict(FirstName='EF Buyer', LastName='Persistence', City='Одеса')),
            ('Sellers', seller, dict(Name='EF Seller', City='Одеса', Description='Updated shop'))]:
            fields = Inputs(request(f'/{controller}/Edit/{item}')[2]).fields
            status, _, body = request(f'/{controller}/Edit/{item}', dict(fields, **values))
            assert status == 200 and 'Одеса' in body
        create_record('Customers', dict(FirstName='EF Buyer', LastName='Alpha', City='Київ'))
        body = request('/Customers?search=EF+Buyer&sort=name_desc')[2]
        assert body.index('EF Buyer Persistence') < body.index('EF Buyer Alpha')
        body = request('/Customers?search=EF+Buyer&sort=name')[2]
        assert body.index('EF Buyer Alpha') < body.index('EF Buyer Persistence')
        product_data = dict(Name='Persistent EF item', Game='Test game', Category='Skins', Price='130.25', Description='Persistent listing', SellerName='EF Seller', IsAvailable='true', __Invariant='Price')
        status, url, body = request('/Products/Create', dict(product_data, __RequestVerificationToken=token('/Products/Create')))
        assert status == 200 and 'details-card' in body
        product = int(url.rsplit('/', 1)[1])
        status, _, body = request(f'/Orders/Create?sellerId={seller}')
        assert status == 200 and 'Persistent EF item' in body and 'EF Buyer Persistence' in body
        order_form = Inputs(body).fields
        order_form.update(CustomerId=str(customer), SellerId=str(seller), **{'Rows[0].Quantity': '2', 'Rows[0].UnitPrice': '0', 'Rows[0].Name': 'Forged'})
        # Invalid quantity and a foreign product must not create an order.
        bad = dict(order_form, **{'Rows[0].Quantity': '-1'})
        assert 'validation-summary-errors' in request('/Orders/Create', bad)[2]
        bad = dict(order_form, **{'Rows[0].ProductId': '1'})
        assert 'validation-summary-errors' in request('/Orders/Create', bad)[2]
        status, url, body = request('/Orders/Create', order_form)
        assert status == 200 and re.search(r'260[.,]50', body) and 'Forged' not in body, body
        order_id = int(url.rsplit('/', 1)[1])
        assert 'EF Seller' in request('/Orders')[2]
        # Foreign-key protected deletes remain user-friendly and preserve data.
        for controller, item in [('Products', product), ('Customers', customer), ('Sellers', seller)]:
            status, _, body = request(f'/{controller}/Delete/{item}', {'Id': item, '__RequestVerificationToken': token(f'/{controller}/Delete/{item}')})
            assert status == 200 and 'validation-summary-errors' in body
            assert request(f'/{controller}/Details/{item}')[0] == 200
        # Catalog repricing does not change an existing order's price snapshot.
        edited = dict(product_data, Price='200', Id=product, __RequestVerificationToken=token(f'/Products/Edit/{product}'))
        assert request(f'/Products/Edit/{product}', edited)[0] == 200
        assert re.search(r'260[.,]50', request(f'/Orders/Details/{order_id}')[2])
        status, _, body = request(f'/Orders/Edit/{order_id}')
        order_edit = Inputs(body).fields
        order_edit.update(CustomerId=str(customer), **{'Rows[0].Quantity': '3'})
        assert re.search(r'390[.,]75', request(f'/Orders/Edit/{order_id}', order_edit)[2])
        # Stale editing is rejected rather than silently overwriting the order.
        assert 'validation-summary-errors' in request(f'/Orders/Edit/{order_id}', order_edit)[2]

        # Filtering, sorting and paging use database-backed directories.
        for i in range(6):
            create_record('Sellers', dict(Name=f'PageSeller{i}', City='Київ'))
        body = request('/Sellers?search=PageSeller&page=1')[2]
        assert body.count('/Sellers/Details/') == 5 and 'Сторінка 1 з 2' in body
        body = request('/Sellers?search=PageSeller&page=2')[2]
        assert body.count('/Sellers/Details/') == 1 and 'Сторінка 2 з 2' in body
        assert 'EF Buyer Persistence' in request('/Customers?search=Persistence&sort=city_desc')[2]
        with sqlite3.connect(database) as connection:
            assert connection.execute('SELECT COUNT(*) FROM __EFMigrationsHistory').fetchone()[0] == 1
            assert connection.execute('SELECT COUNT(*) FROM Orders').fetchone()[0] == 1
        stop()
        server = start()
        assert 'Persistent EF item' in request(f'/Products/Details/{product}')[2]
        assert re.search(r'390[.,]75', request(f'/Orders/Details/{order_id}')[2])
        with sqlite3.connect(database) as connection:
            assert connection.execute("SELECT COUNT(*) FROM Users WHERE DisplayName = 'QueryPlayerUA'").fetchone()[0] == 1
        # Delete the order (cascades its lines), then the related records can be deleted.
        deletion = Inputs(request(f'/Orders/Delete/{order_id}')[2]).fields
        assert request(f'/Orders/Delete/{order_id}', deletion)[0] == 200
        assert request(f'/Orders/Details/{order_id}')[0] == 404
        with sqlite3.connect(database) as connection:
            assert connection.execute('SELECT COUNT(*) FROM OrderLines').fetchone()[0] == 0
        for controller, item in [('Products', product), ('Customers', customer), ('Sellers', seller)]:
            assert request(f'/{controller}/Delete/{item}', {'Id': item, '__RequestVerificationToken': token(f'/{controller}/Delete/{item}')})[0] == 200
            assert request(f'/{controller}/Details/{item}')[0] == 404
        stop()
        server = start()
        assert request(f'/Products/Details/{product}')[0] == 404
        print('PASS: EF migration, relational CRUD, linked orders, server prices, historical totals, stale edits, restricted deletes, search/paging, cascade and restart persistence')
    finally:
        stop()
        logfile.close()
