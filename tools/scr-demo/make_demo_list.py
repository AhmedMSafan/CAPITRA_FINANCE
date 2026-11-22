# Builds database/scripts/demo-screening-list.sql: about 400 people on the Screening Final List with the published decisions that
# list them, each decision with its letter and an Arabic image of the decision (clearly marked as demo data).
# Run: python3 tools/scr-demo/make_demo_list.py   (needs Pillow with raqm for Arabic)
import base64, datetime as dt, hashlib, io, os, random
from PIL import Image, ImageDraw, ImageFont

random.seed(20261122)
ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
OUT_SQL = os.path.join(ROOT, 'database', 'scripts', 'demo-screening-list.sql')
OUT_IMG = os.path.join(ROOT, 'tools', 'scr-demo', 'images')
os.makedirs(OUT_IMG, exist_ok=True)

# ---------------------------------------------------------------- names (Arabic, English, other English spellings)
MALE = [('محمد', 'Mohamed', 'Mohammed'), ('أحمد', 'Ahmed', 'Ahmad'), ('محمود', 'Mahmoud', 'Mahmood'), ('مصطفى', 'Mostafa', 'Mustafa'),
        ('علي', 'Ali', 'Aly'), ('حسن', 'Hassan', 'Hasan'), ('حسين', 'Hussein', 'Hussin'), ('إبراهيم', 'Ibrahim', 'Ebrahim'),
        ('خالد', 'Khaled', 'Khalid'), ('عمر', 'Omar', 'Omer'), ('يوسف', 'Youssef', 'Yousef'), ('طارق', 'Tarek', 'Tariq'),
        ('كريم', 'Karim', 'Kareem'), ('سامح', 'Sameh', None), ('هشام', 'Hesham', 'Hisham'), ('وليد', 'Walid', 'Waleed'),
        ('عمرو', 'Amr', 'Amro'), ('شريف', 'Sherif', 'Shereef'), ('ياسر', 'Yasser', 'Yasir'), ('أيمن', 'Ayman', 'Aiman'),
        ('عادل', 'Adel', 'Adil'), ('سعيد', 'Saeed', 'Said'), ('رامي', 'Ramy', 'Rami'), ('تامر', 'Tamer', None),
        ('عبد الله', 'Abdallah', 'Abdullah'), ('عبد الرحمن', 'Abdelrahman', 'Abdul Rahman'), ('عبد العزيز', 'Abdelaziz', 'Abdul Aziz'),
        ('مجدي', 'Magdy', 'Magdi'), ('فتحي', 'Fathy', 'Fathi'), ('صلاح', 'Salah', None), ('جمال', 'Gamal', 'Jamal'),
        ('نبيل', 'Nabil', 'Nabeel'), ('سمير', 'Samir', 'Sameer'), ('أشرف', 'Ashraf', None), ('حازم', 'Hazem', 'Hazim'),
        ('ماجد', 'Maged', 'Majed'), ('عصام', 'Essam', 'Issam'), ('رضا', 'Reda', 'Ridha'), ('فؤاد', 'Fouad', 'Fuad'), ('منير', 'Mounir', 'Munir')]
FEMALE = [('فاطمة', 'Fatma', 'Fatima'), ('مريم', 'Mariam', 'Maryam'), ('نور', 'Nour', 'Noor'), ('سارة', 'Sara', 'Sarah'),
          ('منى', 'Mona', 'Muna'), ('هدى', 'Hoda', 'Huda'), ('دينا', 'Dina', None), ('رانيا', 'Rania', 'Ranya'),
          ('ياسمين', 'Yasmin', 'Yasmeen'), ('سلمى', 'Salma', None), ('ريهام', 'Reham', 'Riham'), ('نادية', 'Nadia', 'Nadya'),
          ('هالة', 'Hala', None), ('أمل', 'Amal', None), ('إيمان', 'Eman', 'Iman'), ('شيماء', 'Shaimaa', 'Shaymaa'),
          ('نرمين', 'Nermin', 'Nermeen'), ('غادة', 'Ghada', None), ('عبير', 'Abeer', 'Abir'), ('سحر', 'Sahar', None)]
LAST = [('الشافعي', 'El Shafei', 'Al Shafie'), ('المصري', 'El Masry', 'Al Masri'), ('عبد الحميد', 'Abdel Hamid', 'Abdul Hamid'),
        ('السيد', 'El Sayed', 'Elsayed'), ('النجار', 'El Naggar', 'Al Najjar'), ('الشربيني', 'El Sherbiny', None),
        ('عبد الفتاح', 'Abdel Fattah', 'Abdul Fattah'), ('حجازي', 'Hegazy', 'Hijazi'), ('سليمان', 'Soliman', 'Sulaiman'),
        ('رمضان', 'Ramadan', None), ('منصور', 'Mansour', 'Mansur'), ('عثمان', 'Osman', 'Othman'), ('زكي', 'Zaki', 'Zaky'),
        ('البنا', 'El Banna', 'Al Banna'), ('الجمال', 'El Gammal', 'Al Jammal'), ('فرج', 'Farag', 'Faraj'), ('شاهين', 'Shahin', 'Shaheen'),
        ('القاضي', 'El Kady', 'Al Qadi'), ('درويش', 'Darwish', None), ('عبد الغني', 'Abdel Ghani', 'Abdul Ghani'),
        ('الحسيني', 'El Hosseiny', 'Al Husseini'), ('مرسي', 'Morsy', 'Mursi'), ('بدوي', 'Badawy', 'Badawi'), ('خليل', 'Khalil', None),
        ('صبري', 'Sabry', 'Sabri'), ('الدسوقي', 'El Desouky', 'Al Dusuqi'), ('غنيم', 'Ghoneim', 'Ghunaim'), ('الطويل', 'El Tawil', None),
        ('عبد المنعم', 'Abdel Moneim', 'Abdul Munim'), ('الفقي', 'El Feki', 'Al Fiqi')]
GOVS = ['01', '01', '01', '02', '02', '03', '04', '11', '12', '13', '14', '15', '16', '17', '18', '19', '21', '21', '22', '23', '24', '25', '26', '27', '28', '29', '31', '88']

used_ids, used_names = set(), set()


def national_id(birth: dt.date, male: bool) -> str:
    # Egyptian national ID: century (2 = 1900s, 3 = 2000s), YYMMDD, governorate, 3-digit serial, gender digit (odd = male), check digit
    while True:
        serial = f'{random.randint(0, 999):03d}'
        g = random.choice([1, 3, 5, 7, 9] if male else [0, 2, 4, 6, 8])
        body = f'{2 if birth.year < 2000 else 3}{birth:%y%m%d}{random.choice(GOVS)}{serial}{g}'
        nid = body + str(random.randint(1, 9))
        if nid not in used_ids:
            used_ids.add(nid)
            return nid


def person():
    while True:
        male = random.random() < 0.78
        first = random.choice(MALE if male else FEMALE)
        father, grand = random.sample(MALE, 2)
        last = random.choice(LAST)
        parts = [first, father, grand, last] if random.random() < 0.6 else [first, father, last]
        ar = ' '.join(p[0] for p in parts)
        if ar in used_names or first == father:
            continue
        used_names.add(ar)
        en = ' '.join(p[1] for p in parts)
        alias = None
        if random.random() < 0.3:
            alt = [p[2] if p[2] and random.random() < 0.7 else p[1] for p in parts]
            alias = ' '.join(alt) if ' '.join(alt) != en else None
        birth = dt.date(random.randint(1955, 2003), random.randint(1, 12), random.randint(1, 28))
        return {'ar': ar, 'en': en, 'alias': alias, 'birth': birth, 'id': national_id(birth, male)}


# ---------------------------------------------------------------- decisions
SENDERS = [  # (Arabic, English, official number prefix, letter channel)
    ('النيابة العامة', 'Public Prosecution', 'PP-FRZ', 'COURIER'),
    ('محكمة القاهرة الاقتصادية', 'Cairo Economic Court', 'CEC', 'COURIER'),
    ('وحدة مكافحة غسل الأموال وتمويل الإرهاب', 'Money Laundering and Terrorist Financing Combating Unit', 'MLU', 'EMAIL'),
    ('محكمة استئناف القاهرة', 'Cairo Court of Appeal', 'CCA', 'HAND'),
    ('محكمة الإسكندرية الاقتصادية', 'Alexandria Economic Court', 'AEC', 'COURIER'),
    ('جهاز الكسب غير المشروع', 'Illicit Gains Authority', 'IGA', 'COURIER'),
]
LISTING_SCOPES = [
    'تجميد جميع الأموال والحسابات والأصول المملوكة للمذكورين وحظر التعامل معهم',
    'حظر التعامل والمنع من التصرف في الأموال بموجب حكم قضائي نهائي',
    'التحفظ على الأموال في قضية غسل أموال والمنع من فتح حسابات أو منح تمويل',
    'المنع من التصرف في الأموال المنقولة والعقارية لحين الفصل في الدعوى',
    'حكم نهائي بإشهار الإفلاس وحظر التعامل المالي',
]
N_LISTINGS = 22
TOTAL = 400
sizes = [8] * N_LISTINGS
for _ in range(TOTAL - sum(sizes)):
    sizes[random.randrange(N_LISTINGS)] += 1
assert sum(sizes) == TOTAL and max(sizes) <= 34

start, end = dt.date(2024, 1, 15), dt.date(2026, 11, 10)
dates = sorted(start + dt.timedelta(days=random.randint(0, (end - start).days)) for _ in range(N_LISTINGS + 3))
decisions = []
for i in range(N_LISTINGS):
    s = SENDERS[i % len(SENDERS)] if i < len(SENDERS) else random.choice(SENDERS)
    d = dates[i]
    decisions.append({'key': i, 'type': 'LISTING', 'sender': s, 'date': d, 'no': f'{s[2]}-{random.randint(100, 999)}/{d.year}',
                      'scope': random.choice(LISTING_SCOPES), 'people': [person() for _ in range(sizes[i])], 'parent': None})
# a suspension, a lifting and an amendment of earlier listings, so the list also shows Suspended and Lifted entries
late = dates[N_LISTINGS:]
for k, (typ, parent, scope, pick) in enumerate([
        ('SUSPENSION', 3, 'إيقاف تنفيذ قرار التجميد مؤقتًا لحين الفصل في التظلم', 4),
        ('LIFTING', 5, 'رفع الأسماء من القائمة بموجب حكم بالبراءة من محكمة الاستئناف', 5),
        ('AMENDMENT', 7, 'تعديل نطاق القرار ليشمل المنع من السفر إلى جانب تجميد الأموال والحسابات', None)]):
    p = decisions[parent]
    s = SENDERS[3] if typ == 'LIFTING' else p['sender']
    d = max(late[k], p['date'] + dt.timedelta(days=30))
    if d > dt.date(2026, 11, 20):
        d = dt.date(2026, 11, 20)
    decisions.append({'key': N_LISTINGS + k, 'type': typ, 'sender': s, 'date': d, 'no': f'{s[2]}-{random.randint(100, 999)}/{d.year}',
                      'scope': scope, 'people': p['people'] if pick is None else p['people'][:pick], 'parent': parent})
decisions.sort(key=lambda x: (x['date'], x['key']))

# ---------------------------------------------------------------- Arabic decision images
F_REG = '/usr/share/fonts/truetype/freefont/FreeSerif.ttf'
F_BOLD = '/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf'
F_SANS = '/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf'
W, H = 1240, 1754  # A4 at 150 dpi
INK, GREY, LINE = (24, 32, 48), (95, 105, 120), (150, 160, 175)
TYPE_AR = {'LISTING': 'قرار إدراج', 'SUSPENSION': 'قرار إيقاف', 'LIFTING': 'قرار رفع', 'AMENDMENT': 'قرار تعديل'}
AR_DIGITS = str.maketrans('0123456789', '٠١٢٣٤٥٦٧٨٩')


MONTHS = ['يناير', 'فبراير', 'مارس', 'أبريل', 'مايو', 'يونيو', 'يوليو', 'أغسطس', 'سبتمبر', 'أكتوبر', 'نوفمبر', 'ديسمبر']


def ar_date(d):
    return f"{str(d.day).translate(AR_DIGITS)} {MONTHS[d.month - 1]} {str(d.year).translate(AR_DIGITS)}"


def font(path, size):
    return ImageFont.truetype(path, size, layout_engine=ImageFont.Layout.RAQM)


def rtext(draw, x_right, y, text, f, fill=INK):
    # right-aligned Arabic text ending at x_right
    w = draw.textlength(text, font=f, direction='rtl', language='ar')
    draw.text((x_right - w, y), text, font=f, fill=fill, direction='rtl', language='ar')


def ctext(draw, y, text, f, fill=INK):
    w = draw.textlength(text, font=f, direction='rtl', language='ar')
    draw.text(((W - w) / 2, y), text, font=f, fill=fill, direction='rtl', language='ar')


def wrap(draw, text, f, width):
    lines, cur = [], ''
    for word in text.split():
        t = (cur + ' ' + word).strip()
        if draw.textlength(t, font=f, direction='rtl', language='ar') > width and cur:
            lines.append(cur)
            cur = word
        else:
            cur = t
    return lines + [cur]


def ar_no(no):
    num, year = no.split('-')[-1].split('/')
    return f'{num.translate(AR_DIGITS)} لسنة {year.translate(AR_DIGITS)}'


def render(dec, parent_no):
    img = Image.new('RGB', (W, H), (252, 250, 245))
    dr = ImageDraw.Draw(img)
    R, L = W - 90, 90
    # letterhead
    rtext(dr, R, 70, 'جمهورية مصر العربية', font(F_BOLD, 30))
    rtext(dr, R, 115, dec['sender'][0], font(F_BOLD, 34))
    dr.text((L, 78), f"No. {dec['no']}", font=font(F_SANS, 24), fill=GREY)
    dr.text((L, 115), f"Date {dec['date']:%d/%m/%Y}", font=font(F_SANS, 24), fill=GREY)
    dr.line((L, 175, R, 175), fill=INK, width=3)
    dr.line((L, 182, R, 182), fill=LINE, width=1)
    num, year = dec['no'].split('-')[-1].split('/')
    ctext(dr, 215, f"{TYPE_AR[dec['type']]} رقم {num.translate(AR_DIGITS)} لسنة {year.translate(AR_DIGITS)}", font(F_BOLD, 40))
    ctext(dr, 275, f"صادر في {ar_date(dec['date'])}", font(F_REG, 30), GREY)
    y = 345
    body = font(F_REG, 30)
    rtext(dr, R, y, 'السيد / رئيس مجلس إدارة الاتحاد', font(F_BOLD, 28))
    y += 55
    rtext(dr, R, y, 'تحية طيبة وبعد،', body)
    y += 50
    intro = {'LISTING': f"نتشرف بإحاطتكم علمًا بصدور القرار المشار إليه بشأن: {dec['scope']}، وذلك بالنسبة للأشخاص الواردة أسماؤهم أدناه، "
                        'برجاء التعميم على الشركات الأعضاء واتخاذ اللازم نحو التنفيذ.',
             'SUSPENSION': f"إلحاقًا بالقرار رقم {ar_no(parent_no) if parent_no else ''}، نحيطكم علمًا بأنه تقرر: {dec['scope']}، بالنسبة للأشخاص الواردة أسماؤهم أدناه.",
             'LIFTING': f"إلحاقًا بالقرار رقم {ar_no(parent_no) if parent_no else ''}، نحيطكم علمًا بأنه تقرر: {dec['scope']}، بالنسبة للأشخاص الواردة أسماؤهم أدناه.",
             'AMENDMENT': f"إلحاقًا بالقرار رقم {ar_no(parent_no) if parent_no else ''}، نحيطكم علمًا بأنه تقرر: {dec['scope']}، وذلك لجميع الأشخاص الوارد ذكرهم بالقرار."}[dec['type']]
    for line in wrap(dr, intro, body, R - L):
        rtext(dr, R, y, line, body)
        y += 46
    y += 20
    # table of the persons
    people = dec['people']
    row_h = min(42, (H - 360 - y) // (len(people) + 1))
    fsz = 24 if row_h >= 40 else 22
    cols = [(R, 70, 'م'), (R - 70, 520, 'الاسم'), (R - 590, 290, 'الرقم القومي'), (R - 880, 180, 'تاريخ الميلاد')]
    dr.rectangle((L, y, R, y + row_h), fill=(228, 233, 238))
    for x, w, t in cols:
        rtext(dr, x - 12, y + 7, t, font(F_BOLD, fsz - 2))
    y += row_h
    tf, nf = font(F_REG, fsz + 3), font(F_SANS, fsz - 2)
    for i, p in enumerate(people, 1):
        rtext(dr, cols[0][0] - 14, y + (row_h - 34) // 2, str(i).translate(AR_DIGITS), tf)
        rtext(dr, cols[1][0] - 12, y + (row_h - 38) // 2, p['ar'], tf)
        dr.text((cols[2][0] - 278, y + (row_h - 26) // 2), p['id'], font=nf, fill=INK)
        dr.text((cols[3][0] - 168, y + (row_h - 26) // 2), f"{p['birth']:%d/%m/%Y}", font=nf, fill=INK)
        dr.line((L, y + row_h, R, y + row_h), fill=LINE, width=1)
        y += row_h
    # signature
    y += 35
    rtext(dr, R, y, 'وتفضلوا بقبول فائق الاحترام،', body)
    dr.text((L + 40, y + 60), 'التوقيع', font=font(F_BOLD, 26), fill=GREY, direction='rtl', language='ar')
    dr.line((L + 10, y + 140, L + 330, y + 140), fill=GREY, width=1)
    dr.ellipse((L + 380, y + 30, L + 540, y + 190), outline=(120, 40, 40), width=3)
    sf = font(F_BOLD, 18)
    w = dr.textlength('خاتم الجهة', font=sf, direction='rtl', language='ar')
    dr.text((L + 460 - w / 2, y + 98), 'خاتم الجهة', font=sf, fill=(120, 40, 40), direction='rtl', language='ar')
    # demo marking: footer and a diagonal watermark
    dr.line((L, H - 95, R, H - 95), fill=LINE, width=1)
    ctext(dr, H - 82, 'نموذج تجريبي لأغراض الاختبار فقط — الأسماء والأرقام وهمية ولا تمثل أشخاصًا حقيقيين', font(F_REG, 24), (170, 40, 40))
    mark = Image.new('RGBA', (W, H), (0, 0, 0, 0))
    md = ImageDraw.Draw(mark)
    mf = font(F_BOLD, 150)
    t = 'نموذج تجريبي'
    w = md.textlength(t, font=mf, direction='rtl', language='ar')
    md.text(((W - w) / 2, H / 2 - 90), t, font=mf, fill=(200, 40, 40, 38), direction='rtl', language='ar')
    mark = mark.rotate(32, center=(W / 2, H / 2))
    img = Image.alpha_composite(img.convert('RGBA'), mark).convert('L')  # a grey "scan"
    buf = io.BytesIO()
    img.quantize(colors=16).save(buf, 'PNG', optimize=True)
    return buf.getvalue()


# ---------------------------------------------------------------- SQL
def q(s):
    return "NULL" if s is None else "'" + str(s).replace("'", "''") + "'"


sql = []
emit = sql.append
emit('''-- =====================================================================================================================
-- Capitra Screening: demo Final List of about 400 people with the published decisions that list them.
-- Run in pgAdmin > Query Tool on CAPITRA_DB. Press F5. Adds only; deletes and changes nothing that exists.
-- Run it once: if the demo letters are already there (sender reference DEMO-...) it stops and adds nothing.
--
-- What it adds (all clearly demo data: made-up names and national IDs, images marked "نموذج تجريبي"):
--   * %d letters (one per decision), each with its decision as an Arabic image (PNG), status Indexed
--   * %d published decisions: %d listings, 1 suspension, 1 lifting and 1 amendment of earlier listings,
--     each with the same image as a data-entry document, each publishing the next Final List version
--   * %d people (persons, Egyptian 14-digit national IDs that match their birth dates), about a third with another English spelling
-- The decisions show as entered, reviewed and approved by the user names below; change them before running if you like.
-- Generated by tools/scr-demo/make_demo_list.py.
-- =====================================================================================================================
''' % (len(decisions), len(decisions), N_LISTINGS, TOTAL))
emit('''DO $$
DECLARE
  v_entry    text := 'admin';                  -- shown as "entered by"
  v_review   text := 'admin';                  -- shown as "reviewed by"
  v_approve  text := 'admin';                  -- shown as "approved by"
  v_person   text := 'System Administrator';
  v_letter   bigint;
  v_dec      bigint;
  v_party    bigint;
  v_ver      int;
  v_parent   bigint;
  v_img      bytea;
BEGIN
  IF EXISTS (SELECT 1 FROM "SCR_LETTERS" WHERE "SENDER_REFERENCE" LIKE 'DEMO-%') THEN
    RAISE EXCEPTION 'The demo Final List is already loaded (letters with sender reference DEMO-...). Nothing was added.';
  END IF;
''')
dec_no_of_key = {d['key']: d['no'] for d in decisions}
for n, d in enumerate(decisions, 1):
    parent_no = dec_no_of_key.get(d['parent']) if d['parent'] is not None else None
    png = render(d, parent_no)
    fname = f"DEMO-{n:02d}-{d['type'].lower()}-{d['no'].replace('/', '-')}.png"
    open(os.path.join(OUT_IMG, fname), 'wb').write(png)
    sha = hashlib.sha256(png).hexdigest()
    received = dt.datetime.combine(d['date'] - dt.timedelta(days=random.randint(1, 4)), dt.time(9, random.randint(0, 59)))
    letter_date = received.date() - dt.timedelta(days=random.randint(1, 3))
    pub = dt.datetime.combine(d['date'], dt.time(12, 0))
    emit(f"\n  -- {n}. {d['type']} {d['no']} · {d['sender'][1]} · {d['date']} · {len(d['people'])} people")
    emit(f"  v_img := decode('{base64.b64encode(png).decode()}', 'base64');")
    emit(f'''  INSERT INTO "SCR_LETTERS" ("LETTER_NO","SENDER_REFERENCE","SENDER_NAME","CHANNEL_CODE","LETTER_DATE","RECEIVED_AT","STATUS_CODE","NOTES","CREATED_BY","CREATED_AT","ROW_VERSION")
    VALUES ('PENDING', {q(f"DEMO-{n:02d}/{d['no']}")}, {q(d['sender'][0])}, {q(d['sender'][3])}, DATE '{letter_date}', TIMESTAMPTZ '{received:%Y-%m-%d %H:%M}:00+02', 'INDEXED', 'Demo data', v_entry, TIMESTAMPTZ '{received:%Y-%m-%d %H:%M}:00+02', 0)
    RETURNING "LETTER_ID" INTO v_letter;
  UPDATE "SCR_LETTERS" SET "LETTER_NO" = 'INT-' || lpad(v_letter::text, 6, '0') WHERE "LETTER_ID" = v_letter;
  INSERT INTO "SCR_LETTER_FILES" ("LETTER_ID","FILE_NAME","MIME_TYPE","FILE_SIZE_BYTES","SHA256_HASH","FILE_BLOB","CREATED_BY","CREATED_AT","ROW_VERSION")
    VALUES (v_letter, {q(fname)}, 'image/png', {len(png)}, '{sha}', v_img, v_entry, TIMESTAMPTZ '{received:%Y-%m-%d %H:%M}:00+02', 0);''')
    if d['parent'] is not None:
        emit(f'''  SELECT "DECISION_ID" INTO v_parent FROM "SCR_DECISIONS" WHERE "OFFICIAL_NO" = {q(parent_no)} AND "DECISION_TYPE_CODE" = 'LISTING' ORDER BY "DECISION_ID" DESC LIMIT 1;''')
    emit(f'''  SELECT COALESCE(MAX("VERSION_NO"), 0) + 1 INTO v_ver FROM "SCR_REGISTRY_VERSIONS";
  INSERT INTO "SCR_DECISIONS" ("DECISION_REF","OFFICIAL_NO","DECISION_TYPE_CODE","PARENT_DECISION_ID","LETTER_ID","SCOPE_TEXT","EFFECTIVE_DATE","STATUS_CODE",
      "ENTERED_BY","ENTERED_BY_PERSON","REVIEWED_BY","REVIEWED_BY_PERSON","REVIEWED_AT","APPROVED_BY","APPROVED_BY_PERSON","PUBLISHED_AT","REGISTRY_VERSION_NO","CREATED_BY","CREATED_AT","ROW_VERSION")
    VALUES ('PENDING', {q(d['no'])}, '{d['type']}', {'v_parent' if d['parent'] is not None else 'NULL'}, v_letter, {q(d['scope'])}, DATE '{d['date']}', 'PUBLISHED',
      v_entry, v_person, v_review, v_person, TIMESTAMPTZ '{pub:%Y-%m-%d} 10:00:00+02', v_approve, v_person, TIMESTAMPTZ '{pub:%Y-%m-%d %H:%M}:00+02', v_ver, v_entry, TIMESTAMPTZ '{received:%Y-%m-%d %H:%M}:00+02', 0)
    RETURNING "DECISION_ID" INTO v_dec;
  UPDATE "SCR_DECISIONS" SET "DECISION_REF" = 'DEC-' || lpad(v_dec::text, 6, '0') WHERE "DECISION_ID" = v_dec;
  INSERT INTO "SCR_REGISTRY_VERSIONS" ("VERSION_NO","DECISION_ID","PUBLISHED_AT","PUBLISHED_BY","CREATED_BY","CREATED_AT","ROW_VERSION")
    VALUES (v_ver, v_dec, TIMESTAMPTZ '{pub:%Y-%m-%d %H:%M}:00+02', v_approve, v_approve, TIMESTAMPTZ '{pub:%Y-%m-%d %H:%M}:00+02', 0);
  INSERT INTO "SCR_DECISION_DOCUMENTS" ("DECISION_ID","WORKFLOW_INSTANCE_ID","STEP_CODE","STEP_NAME","DOCUMENT_TITLE","FILE_NAME","MIME_TYPE","FILE_SIZE_BYTES","SHA256_HASH","FILE_BLOB","IS_ACTIVE","CREATED_BY","CREATED_AT","ROW_VERSION")
    VALUES (v_dec, NULL, 'DATA_ENTRY', 'Data entry', {q('صورة القرار ' + d['no'])}, {q(fname)}, 'image/png', {len(png)}, '{sha}', v_img, true, v_entry, TIMESTAMPTZ '{received:%Y-%m-%d %H:%M}:00+02', 0);''')
    for p in d['people']:
        if d['type'] == 'LISTING':
            emit(f'''  v_party := NULL;
  INSERT INTO "SCR_PARTIES" ("PARTY_TYPE_CODE","NAME_AR","NAME_EN","IDENTITY_TYPE_CODE","IDENTITY_NO","NATIONALITY_CODE","BIRTH_DATE","NOTES","CREATED_BY","CREATED_AT","ROW_VERSION")
    VALUES ('PERSON', {q(p['ar'])}, {q(p['en'])}, 'NATIONAL_ID', '{p['id']}', 'EG', DATE '{p['birth']}', 'Demo data', v_entry, now(), 0)
    ON CONFLICT ("PARTY_TYPE_CODE","IDENTITY_NO") DO NOTHING RETURNING "PARTY_ID" INTO v_party;''')
            if p['alias']:
                emit(f'''  IF v_party IS NOT NULL THEN INSERT INTO "SCR_PARTY_ALIASES" ("PARTY_ID","ALIAS_NAME","CREATED_BY","CREATED_AT","ROW_VERSION") VALUES (v_party, {q(p['alias'])}, v_entry, now(), 0); END IF;''')
            emit(f'''  IF v_party IS NULL THEN SELECT "PARTY_ID" INTO v_party FROM "SCR_PARTIES" WHERE "PARTY_TYPE_CODE" = 'PERSON' AND "IDENTITY_NO" = '{p['id']}'; END IF;''')
        else:
            emit(f'''  SELECT "PARTY_ID" INTO v_party FROM "SCR_PARTIES" WHERE "PARTY_TYPE_CODE" = 'PERSON' AND "IDENTITY_NO" = '{p['id']}';''')
        emit('''  INSERT INTO "SCR_DECISION_PARTIES" ("DECISION_ID","PARTY_ID","CREATED_BY","CREATED_AT","ROW_VERSION") VALUES (v_dec, v_party, v_entry, now(), 0) ON CONFLICT DO NOTHING;''')

emit(f'''
  RAISE NOTICE 'Demo Final List loaded: {len(decisions)} letters and published decisions, {TOTAL} people.';
END $$;''')
open(OUT_SQL, 'w', encoding='utf-8').write('\n'.join(sql) + '\n')
print('decisions', len(decisions), 'people', TOTAL, 'sql bytes', os.path.getsize(OUT_SQL))
for d in decisions:
    print(d['date'], d['type'], d['no'], len(d['people']))
