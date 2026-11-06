"""Practice 4 first phase; also runs the regression tests from smoke.py."""
import re
from decimal import Decimal
from html.parser import HTMLParser
from urllib.parse import urlsplit
from smoke import request, token, base

class Inputs(HTMLParser):
    def __init__(self, source):
        super().__init__()
        self.fields = {}
        self.feed(source)
    def handle_starttag(self, tag, attrs):
        attrs = dict(attrs)
        if tag != 'input' or 'name' not in attrs:
            return
        key = attrs['name']
        value = attrs.get('value', '')
        if key in self.fields:
            previous = self.fields[key]
            self.fields[key] = (previous if isinstance(previous, list) else [previous]) + [value]
        else:
            self.fields[key] = value

valid = dict(Name='Batch test listing', Game='Test game', Category='Skins', Price='10.25', SellerName='Test seller', Description='', IsAvailable='true', __Invariant='Price')

def create(**changes):
    data = dict(valid, **changes)
    data['__RequestVerificationToken'] = token('/Products/Create')
    return request('/Products/Create', data)

for changes in [dict(Price='-0.01'), dict(Price='100000.01'), dict(Name=''), dict(Category='999'), dict(Category=''), dict(Description='ab'), dict(Description='Batch test listing')]:
    status, url, body = create(**changes)
    assert status == 200 and urlsplit(url).path.lower() == '/products/create' and 'field-validation-error' in body, changes

created = []
for price in ['0', '100000', '10.25']:
    status, url, body = create(Price=price)
    assert status == 200 and 'details-card' in body, (price, body)
    created.append(re.search(r'/(?:Products/Details|products)/(\d+)', url)[1])
# The product name need not be a prefix of the description in the marketplace.
status, url, _ = create(Description='Передача через Steam Trade після узгодження.')
assert status == 200 and '/create' not in url.lower()
created.append(re.search(r'/(?:Products/Details|products)/(\d+)', url)[1])

path = '/Products/EditPrices?category=Skins'
status, _, body = request(path)
assert status == 200 and 'Batch test listing' in body
original = Inputs(body).fields
keys = sorted(k for k in original if re.fullmatch(r'Rows\[\d+\]\.Price', k))
assert len(keys) >= 4

def read_prices():
    return {k: v for k, v in Inputs(request(path)[2]).fields.items() if re.fullmatch(r'Rows\[\d+\]\.Price', k)}

before = read_prices()
invalid = dict(original)
invalid[keys[0]] = '20.50'
invalid[keys[1]] = '-1'
status, _, body = request('/Products/EditPrices', invalid)
assert status == 200 and 'field-validation-error' in body and read_prices() == before
# Missing price, foreign IDs, duplicate IDs, and stale original prices must not partially save.
for bad in [dict(original, **{keys[0]: ''}),
            dict(original, **{'Rows[0].Id': '2'}),
            dict(original, **{'Rows[1].Id': original['Rows[0].Id']}),
            dict(original, **{'Rows[0].OriginalPrice': '99999'})]:
    status, _, body = request('/Products/EditPrices', bad)
    assert status == 200 and 'validation-summary-errors' in body and read_prices() == before
assert request('/Products/EditPrices?category=999')[0] == 400
assert request('/Products/EditPrices', {})[0] == 400

edited = dict(original)
for key in keys:
    edited[key] = '20.50'
# Posted display names cannot rename listings.
edited['Rows[0].Name'] = 'FORGED NAME'
status, url, body = request('/Products/EditPrices', edited)
assert status == 200 and 'Усі ціни категорії збережено' in body and 'FORGED NAME' not in body
assert all(Decimal(v) == Decimal('20.50') for v in read_prices().values())
# Restore prices of existing products and remove the test listings.
restore = Inputs(request(path)[2]).fields
for key in keys:
    restore[key] = original[key]
assert request('/Products/EditPrices', restore)[0] == 200
for item_id in created:
    assert request('/Products/Delete/' + item_id, {'Id': item_id, '__RequestVerificationToken': token('/Products/Delete/' + item_id)})[0] == 200

query_path = '/Users/Create?DisplayName=QueryPlayerUA&FavoriteGame=CS2'
status, _, body = request(query_path)
assert status == 200 and len(re.findall(r'<input[^>]*readonly', body)) == 2
fields = Inputs(body).fields
# Form-only fields cannot create a user.
status, _, body = request('/Users/CreateFromQuery', dict(fields, DisplayName='BodyOnly'))
assert status == 200 and 'field-validation-error' in body
# Query is the only source, even when the form body and a role query disagree.
status, url, body = request('/Users/CreateFromQuery?DisplayName=QueryPlayerUA&FavoriteGame=CS2&Role=Admin', dict(fields, DisplayName='ForgedBodyName', Role='Admin'))
assert status == 200 and 'Демопрофіль створено' in body and 'QueryPlayerUA' in body and 'Користувач' in body and 'ForgedBodyName' not in body
# A repeated POST must not create another profile with the same name.
status, _, body = request('/Users/CreateFromQuery?DisplayName=QueryPlayerUA', fields)
assert status == 200 and 'уже використовується' in body
assert request('/Users/CreateFromQuery?DisplayName=OtherPlayer', {})[0] == 400
print('PASS: validation boundaries, enum, optional descriptions, details redirect, atomic batch prices, tampering/stale batches, query-only profiles and fixed role')
