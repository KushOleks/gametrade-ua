"""Run against a locally running instance: python3 tests/smoke.py [base URL]."""
import html
import http.cookiejar
import re
import sys
import urllib.error
import urllib.parse
import urllib.request

base = sys.argv[1] if len(sys.argv) > 1 else 'http://127.0.0.1:5186'
client = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()))

def request(path, data=None):
    try:
        response = client.open(base + path, None if data is None else urllib.parse.urlencode(data).encode())
        return response.status, response.geturl(), re.sub(r"&#[^;]+;", lambda m: html.unescape(m[0]), response.read().decode())
    except urllib.error.HTTPError as error:
        return error.code, error.geturl(), re.sub(r"&#[^;]+;", lambda m: html.unescape(m[0]), error.read().decode())

def token(path):
    status, _, body = request(path)
    assert status == 200, (path, status)
    return re.search(r'name="__RequestVerificationToken"[^>]*value="([^"]+)"', body)[1]

for path in ['/', '/Products', '/Products/Details/1', '/Products/Edit/1', '/Products/Delete/1']:
    assert request(path)[0] == 200, path
for action in ['Details', 'Edit', 'Delete']:
    assert request('/Products/' + action + '/999999')[0] == 404
assert request('/Products/Create', {})[0] == 400
product = dict(Name='Smoke test item', Game='Test game', Category='Скіни', Price='125.75', SellerName='Tester', Description='<script>alert(1)</script>', IsAvailable='true')
product['__Invariant'] = 'Price'
product['__RequestVerificationToken'] = token('/Products/Create')
invalid = dict(product, Name='', Price='0', Category='invalid')
status, _, body = request('/Products/Create', invalid)
assert status == 200 and 'Вкажіть назву товару' in body and 'Оберіть категорію зі списку' in body
status, _, body = request('/Products/Create', product)
assert status == 200 and 'Smoke test item' in body
# Find the created card independently of the seed items.
assert '<h2>Smoke test item</h2>' in body, body
card = body[body.index('<h2>Smoke test item</h2>'):]
item_id = re.search(r'/(?:Products/Details|products)/(\d+)', card)[1]
status, _, body = request('/Products/Details/' + item_id)
assert status == 200 and re.search(r'125[.,]75', body) and '&lt;script&gt;' in body
product.update(Id=item_id, Name='Edited smoke item', IsAvailable='false', __RequestVerificationToken=token('/Products/Edit/' + item_id))
assert request('/Products/Edit/999999', product)[0] == 400
assert request('/Products/Edit/' + item_id, dict(product, Price='0'))[0] == 200
assert 'Smoke test item' in request('/Products/Details/' + item_id)[2]
assert request('/Products/Edit/' + item_id, product)[0] == 200
body = request('/Products/Details/' + item_id)[2]
assert 'Edited smoke item' in body and 'Недоступний' in body
# GET displays confirmation without deleting.
assert token('/Products/Delete/' + item_id)
assert request('/Products/Details/' + item_id)[0] == 200
assert request('/Products/Delete/' + item_id, {'Id': item_id, '__RequestVerificationToken': token('/Products/Delete/' + item_id)})[0] == 200
assert request('/Products/Details/' + item_id)[0] == 404
print('PASS: pages, missing IDs, antiforgery, validation, decimal price, HTML escaping, create/edit/delete, confirmation GET')

# Practice 3: aliases share the same pages and operations.
for path in ['/products/index', '/items/index', '/products/', '/items', '/products/1', '/products/new']:
    assert request(path)[0] == 200, path
for path in ['/products/999999', '/products/edit/999999', '/products/delete/999999']:
    status, _, body = request(path)
    assert status == 404 and 'The product does not exist' in body and 'До каталогу' in body

def catalog(**filters):
    return request('/products?' + urllib.parse.urlencode(filters))

def names(body):
    return re.findall(r'<h2>(.*?)</h2>', body)

assert names(catalog(Search='redLINE')[2]) == ['AK-47 | Redline']
assert names(catalog(Game='World of Warcraft')[2]) == ['1000 Gold']
assert names(catalog(Category='Акаунти')[2]) == ['GTA V Account']
assert names(catalog(MinPrice='650.00', MaxPrice='650.00', OnlyAvailable='true')[2]) == ['AK-47 | Redline']
assert names(catalog(Search='missing')[2]) == []
for filters in [dict(MinPrice='1000', MaxPrice='10'), dict(MinPrice='abc'), dict(MinPrice='-1')]:
    status, _, body = catalog(**filters)
    assert status == 200 and 'validation-summary-errors' in body and names(body) == []
# GET never accepts the exercise code, even in its query string.
assert request('/users/index')[0] == 401
assert request('/users/index?accessCode=df2323eoT')[0] == 401
access_body = request('/users/index')[2]
access_token = re.search(r'name="__RequestVerificationToken"[^>]*value="([^"]+)"', access_body)[1]
for code in ['', 'wrong']:
    assert request('/users/index', {'accessCode': code, '__RequestVerificationToken': access_token})[0] == 401
assert request('/users/index?accessCode=df2323eoT', {'__RequestVerificationToken': access_token})[0] == 401
status, url, body = request('/users/index', {'accessCode': 'df2323eoT', '__RequestVerificationToken': access_token})
assert status == 200 and 'DemoAdmin' in body and '?' not in url and 'df2323eoT' not in body
assert request('/users/index', {'accessCode': 'df2323eoT'})[0] == 400
print('PASS: Practice 3 aliases, custom 404, combined query filters, invalid ranges, body-only admin access, 401 and antiforgery')
