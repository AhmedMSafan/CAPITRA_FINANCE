# One-off: sample journals for the GL manual screenshots, made through the application on an empty test database.
# Two users: Mariam (admin, enters and posts) and Karim (approves: a journal is approved by another person).
import json, sys, time, urllib.request

A = 'http://localhost:5080'


def call(path, body=None, token=None, method=None):
    data = json.dumps(body).encode() if body is not None else None
    req = urllib.request.Request(A + path, data=data, method=method or ('POST' if data else 'GET'))
    req.add_header('Content-Type', 'application/json')
    if token:
        req.add_header('Authorization', 'Bearer ' + token)
    try:
        with urllib.request.urlopen(req) as r:
            t = r.read().decode()
            return json.loads(t) if t else None
    except urllib.error.HTTPError as e:
        print('HTTP', e.code, path, e.read().decode()[:400])
        raise


def login(user, pw):
    return call('/api/sec/auth/login', {'userName': user, 'password': pw})


adm = login('admin', 'Admin#Seed2026')
T = adm['token']
users = call('/api/sec/users', token=T)
me = next(u for u in users if u['userName'] == 'admin')
call(f"/api/sec/users/{me['id']}", {'email': 'mariam@finova.example', 'fullName': 'Mariam Adel', 'employeeNo': 'E-1001', 'department': 'Finance',
                                       'jobTitle': 'Chief accountant', 'isActive': True}, T, 'PUT')
r = call('/api/sec/users', {'userName': 'karim.gl', 'password': 'Karim#Temp2026', 'email': 'karim@finova.example', 'fullName': 'Karim Fawzy',
                            'department': 'Finance', 'jobTitle': 'Finance manager', 'isActive': True, 'roles': ['ADMIN']}, T)
k = login('karim.gl', 'Karim#Temp2026')
call('/api/sec/auth/change-password', {'currentPassword': 'Karim#Temp2026', 'newPassword': 'Karim#Final2026'}, k['token'])
K = login('karim.gl', 'Karim#Final2026')['token']


def apex(page, process, token, **kw):
    time.sleep(1.3)  # posting run numbers come from the clock
    res = call(f'/api/apex/120/{page}/{process}', kw, token)
    if isinstance(res, dict) and res.get('ok') is False:
        print('FAILED', process, json.dumps(res)[:600])
        sys.exit(1)
    return res


def journal(date, name, category, lines, end='POSTED', ref=None):
    body = {'action': 'SUBMIT' if end != 'DRAFT' else 'SAVE', 'ledger_code': 'FINOVA_EGP', 'category': category, 'accounting_date': date, 'name': name,
            'description': name, 'reference_no': ref, 'currency': 'EGP',
            'lines': [{'combination': c, 'dr': dr, 'cr': cr, 'description': d} for c, dr, cr, d in lines]}
    res = apex(50, 'J50_SAVE', T, clob01=json.dumps(body))
    jid = res['id']
    if end in ('APPROVED', 'POSTED', 'REVERSED'):
        apex(50, 'J50_ACTION', K, x01='APPROVE', x02=str(jid), x03='Checked against the supporting documents.')
    if end in ('POSTED', 'REVERSED'):
        apex(50, 'J50_ACTION', T, x01='POST', x02=str(jid))
    if end == 'REVERSED':
        rv = apex(50, 'J50_ACTION', T, x01='REVERSE', x02=str(jid), x03='Wrong amount: the invoice was 950,000.', x04=date)
        nid = rv['newId']
        apex(50, 'J50_ACTION', T, x01='SUBMIT', x02=str(nid), x03='Reversal of a wrong amount')
        apex(50, 'J50_ACTION', K, x01='APPROVE', x02=str(nid), x03='Agreed.')
        apex(50, 'J50_ACTION', T, x01='POST', x02=str(nid))
    print(res.get('journalNo'), name, end)
    return jid


def c(acc, bl='GENERAL', br='HQ', cc='1100'):
    return f'01-{br}-{cc}-{acc}-{bl}-000-000'


journal('2026-10-01', 'Share capital paid in', 'GENERAL', [(c('110101'), 50000000, 0, 'Capital paid in'), (c('310101'), 0, 50000000, 'Share capital')], ref='BOD-2026-07')
journal('2026-10-05', 'NBE facility drawdown', 'GENERAL', [(c('110102'), 20000000, 0, 'Drawdown received'), (c('210101'), 0, 20000000, 'NBE-FAC-2026-01')], ref='NBE-FAC-2026-01')
journal('2026-10-20', 'Factoring disbursements, October', 'GENERAL',
        [(c('120101', 'FACTORING', cc='1200'), 12500000, 0, 'Advances to sellers'), (c('110101'), 0, 12500000, 'Paid from NBE')])
journal('2026-10-25', 'Mortgage loans, October', 'GENERAL',
        [(c('120102', 'MORTGAGE', cc='1200'), 8000000, 0, 'Loans paid out'), (c('110102'), 0, 8000000, 'Paid from CIB')])
journal('2026-10-31', 'Salaries, October', 'ACCRUAL',
        [(c('510101', cc='1100'), 220000, 0, 'Finance'), (c('510101', cc='1200'), 310000, 0, 'Operations'), (c('510101', cc='1300'), 150000, 0, 'Sales'),
         (c('110101'), 0, 680000, 'Payroll transfer')], ref='PAY-2026-10')
journal('2026-10-31', 'Office rent, October', 'GENERAL', [(c('510102'), 120000, 0, 'Head office rent'), (c('110101'), 0, 120000, 'Rent paid')])
journal('2026-11-10', 'Factoring commission and fees, November', 'GENERAL',
        [(c('110101'), 495000, 0, 'Received'), (c('410101', 'FACTORING', cc='1300'), 0, 420000, 'Commission'), (c('410103', 'FACTORING', cc='1300'), 0, 75000, 'Setup fees')])
journal('2026-11-15', 'Interest received, November', 'GENERAL',
        [(c('110102'), 1150000, 0, 'Received'), (c('410102', 'FACTORING', cc='1200'), 0, 700000, 'Factoring'), (c('410102', 'MORTGAGE', cc='1200'), 0, 450000, 'Mortgage')])
journal('2026-11-18', 'Bank interest, November', 'ACCRUAL', [(c('510103'), 380000, 0, 'NBE facility interest'), (c('110102'), 0, 380000, 'Debited by NBE')])
journal('2026-11-19', 'Office equipment, Alexandria', 'GENERAL',
        [(c('150101', br='ALX', cc='1200'), 900000, 0, 'Furniture and IT equipment'), (c('110101'), 0, 900000, 'Supplier payment')], end='REVERSED', ref='INV-ALX-7781')
journal('2026-11-19', 'Office equipment, Alexandria (corrected)', 'ADJUSTMENT',
        [(c('150101', br='ALX', cc='1200'), 950000, 0, 'Furniture and IT equipment'), (c('110101'), 0, 950000, 'Supplier payment')], ref='INV-ALX-7781')
journal('2026-11-21', 'Salaries, November', 'ACCRUAL',
        [(c('510101', cc='1100'), 225000, 0, 'Finance'), (c('510101', cc='1200'), 318000, 0, 'Operations'), (c('510101', cc='1300'), 157000, 0, 'Sales'),
         (c('110101'), 0, 700000, 'Payroll transfer')], end='APPROVED', ref='PAY-2026-11')
journal('2026-11-22', 'Depreciation, November', 'ACCRUAL', [(c('510104'), 75000, 0, 'Monthly depreciation'), (c('150101', br='ALX', cc='1200'), 0, 75000, 'Equipment')], end='SUBMITTED')
journal('2026-11-23', 'Consumer loans, November', 'GENERAL',
        [(c('120103', 'CONSUMER', 'GIZA', '1200'), 2400000, 0, 'Loans paid out'), (c('110101'), 0, 2400000, 'Paid from NBE')], end='DRAFT')
print('done')
