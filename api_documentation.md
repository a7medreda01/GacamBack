# توثيق واجهات برمجة التطبيقات (API) لمشروع GACAM
**الهيئة العامة للإعلام المرئي والمسموع والخليجي والعربي في كندا**

---

## معلومات عامة

| البيان | القيمة |
|---|---|
| **رابط الخادم المحلي** | `http://localhost:5244` |
| **Swagger UI** | `http://localhost:5244/swagger` |
| **نوع الاستجابة الافتراضي** | `application/json` |
| **تشفير البيانات** | UTF-8 |

### كيفية استخدام التوثيق في Angular
1. بعد تسجيل الدخول، احفظ الـ `token` المُعاد.
2. في كل طلب محمي، أضف الـ Header التالي:
   ```
   Authorization: Bearer eyJhbGci...
   ```
3. لرفع الملفات، استخدم `FormData` وليس `JSON`.

### مستويات الصلاحيات
- **`عام`** — لا يحتاج Token
- **`محمي`** — يحتاج Token خاص بأي مستخدم مسجل
- **`Admin`** — يحتاج Token خاص بحساب مسؤول فقط
- **`Admin/Employee`** — يحتاج Token خاص بمسؤول أو موظف

---

## 1. نظام الحسابات والمصادقة (`/api/Auth`)

> هذه الواجهات مسؤولة عن تسجيل الحسابات، تسجيل الدخول، والحصول على بيانات الملف الشخصي.

---

### 1.1 تسجيل مستخدم جديد — `POST /api/Auth/register`

**الصلاحية:** عام | **الوصف:** ينشئ حساباً جديداً على المنصة بكلمة مرور مشفرة.

**المدخلات (Body - JSON):**
```json
{
  "email": "user@example.com",
  "password": "SecurePass123!",
  "fullName": "محمد أحمد",
  "phoneNumber": "+14161234567"
}
```

**المخرجات — 200 OK:**
```json
{
  "message": "Registration successful."
}
```

**أخطاء محتملة:**
- `400` — بيانات ناقصة أو البريد مستخدم مسبقاً.

---

### 1.2 تسجيل الدخول — `POST /api/Auth/login`

**الصلاحية:** عام | **الوصف:** يرسل بيانات الاعتماد ويحصل على رمز JWT صالح لمدة 7 أيام.

**المدخلات (Body - JSON):**
```json
{
  "email": "admin@gacam.media",
  "password": "Admin@Gacam2026"
}
```

**المخرجات — 200 OK:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "admin@gacam.media",
  "fullName": "GACAM Admin",
  "roles": ["Admin"]
}
```

> ⚠️ **مهم:** الـ `token` يُحفظ ويُستخدم في كل طلب محمي عبر Header الـ `Authorization: Bearer <token>`.

---

### 1.3 الملف الشخصي — `GET /api/Auth/profile`

**الصلاحية:** محمي | **الوصف:** يعيد بيانات المستخدم الحالي صاحب الـ Token.

**المخرجات — 200 OK:**
```json
{
  "id": 1,
  "email": "admin@gacam.media",
  "fullName": "GACAM Admin",
  "phoneNumber": "+1234567890",
  "isActive": true,
  "roles": ["Admin"]
}
```

### 1.4 تعيين دور (صلاحية) لمستخدم — `POST /api/Auth/users/{id}/roles`

**الصلاحية:** Admin | **الوصف:** يضيف دوراً معيناً (مثل Employee أو Admin) للمستخدم المحدد بالـ ID.

**المدخلات (Body - JSON):**
```json
{
  "roleName": "Employee"
}
```

**المخرجات — 200 OK:**
```json
{
  "message": "Role 'Employee' assigned to user successfully."
}
```

---

### 1.5 سحب دور (صلاحية) من مستخدم — `DELETE /api/Auth/users/{id}/roles`

**الصلاحية:** Admin | **الوصف:** يحذف دوراً معيناً من أدوار مستخدم محدد بالـ ID.

**المدخلات (Query String):**
- `roleName` — اسم الدور المراد حذفه (مثل `Employee`)

**مثال الطلب:** `DELETE /api/Auth/users/2/roles?roleName=Employee`

**المخرجات — 200 OK:**
```json
{
  "message": "Role 'Employee' removed from user successfully."
}
```

---


## 2. الصفحات الديناميكية (`/api/Pages`)

> الموقع يحتوي على **19 صفحة ديناميكية** تدعم اللغتين العربية والإنجليزية، تُدار بالكامل عبر هذه الواجهات. يتم تعريف كل صفحة بـ `slug` فريد.

### قائمة الـ Slugs المتاحة

| الـ Slug | الصفحة بالإنجليزي | الصفحة بالعربي |
|---|---|---|
| `home` | Home | الرئيسية |
| `about-us` | About Us | من نحن |
| `vision-mission` | Vision & Mission | الرؤية والرسالة |
| `board-members` | Board Members | أعضاء مجلس الإدارة |
| `organizational-chart` | Organizational Chart | الهيكل التنظيمي |
| `media-accreditation` | Media Accreditation | الاعتماد الإعلامي |
| `volunteer` | Volunteer | التطوع |
| `training` | Training Programs | البرامج التدريبية |
| `news` | News & Activities | الأخبار والأنشطة |
| `partners` | Partners | الشركاء |
| `services` | Services | الخدمات |
| `membership` | Membership | العضوية |
| `media-card` | Media Card | بطاقة الاعتماد |
| `gallery` | Gallery | المعرض |
| `faq` | FAQ | الأسئلة الشائعة |
| `terms` | Terms & Conditions | الشروط والأحكام |
| `privacy-policy` | Privacy Policy | سياسة الخصوصية |
| `contact-us` | Contact Us | تواصل معنا |
| `certificates` | Certificates | الشهادات |

---

### 2.1 جلب كل الصفحات — `GET /api/Pages`

**الصلاحية:** عام

**المخرجات — 200 OK:**
```json
[
  {
    "slug": "home",
    "titleEn": "Home",
    "titleAr": "الرئيسية",
    "contentEn": "The Gulf & Arab General Commission...",
    "contentAr": "الهيئة العامة للإعلام المرئي والمسموع..."
  }
]
```

---

### 2.2 جلب صفحة بالـ Slug — `GET /api/Pages/{slug}`

**الصلاحية:** عام | **مثال:** `GET /api/Pages/about-us`

**المخرجات — 200 OK:**
```json
{
  "slug": "about-us",
  "titleEn": "About Us",
  "titleAr": "من نحن",
  "contentEn": "Content in English...",
  "contentAr": "المحتوى بالعربية..."
}
```

**أخطاء:** `404` — الصفحة غير موجودة.

---

### 2.3 تعديل محتوى صفحة — `PUT /api/Pages/{slug}`

**الصلاحية:** Admin | **مثال:** `PUT /api/Pages/about-us`

**المدخلات (Body - JSON):**
```json
{
  "titleEn": "About Us",
  "titleAr": "من نحن",
  "contentEn": "Updated English content...",
  "contentAr": "المحتوى المعدل بالعربية..."
}
```

**المخرجات — 200 OK:** يعيد الكائن بعد التعديل.

---

## 3. رفع الملفات والصور (`/api/Files`)

> **جميع عمليات رفع الملفات في الموقع تتم عبر هذه الواجهة أولاً.** يتم الحصول على `relativePath` ثم تمريره في طلبات الإنشاء الأخرى (اعتماد، تطوع، مدفوعات... إلخ).

---

### 3.1 رفع ملف — `POST /api/Files/upload/{folder}`

**الصلاحية:** محمي | **نوع الطلب:** `multipart/form-data`

**المجلدات المتاحة (`{folder}`):**

| المجلد | الاستخدام |
|---|---|
| `images` | الصور العامة |
| `cvs` | السير الذاتية (PDF) |
| `receipts` | إيصالات الدفع والتحويل |
| `logos` | لوجو الموقع وشعارات الشركاء |
| `news` | صور الأخبار والمقالات |
| `documents` | ملفات عامة أخرى |

**المدخلات (Form-Data):**
- `file` — الملف المراد رفعه (حد أقصى: 10 MB)

**المخرجات — 200 OK:**
```json
{
  "relativePath": "/uploads/cvs/7a9f8b6c-cv.pdf",
  "absoluteUrl": "http://localhost:5244/uploads/cvs/7a9f8b6c-cv.pdf",
  "fileName": "7a9f8b6c-cv.pdf"
}
```

> 💡 **استخدم `relativePath` لحفظها في قاعدة البيانات، و`absoluteUrl` لعرضها مباشرةً في الواجهة.**

---

## 4. الاعتمادات الإعلامية (`/api/Accreditation`)

> نظام إدارة طلبات الاعتماد الإعلامي وإصدار بطاقات الصحفيين.

---

### 4.1 تقديم طلب اعتماد — `POST /api/Accreditation/apply`

**الصلاحية:** محمي | **الوصف:** يقدم المستخدم طلب اعتماد بعد رفع ملفاته عبر واجهة Files.

**المدخلات (Body - JSON):**
```json
{
  "fullName": "الاسم الكامل على البطاقة",
  "organization": "قناة الجزيرة",
  "jobTitle": "مصور صحفي",
  "nationalIdOrPassport": "A12345678",
  "cvUrl": "/uploads/cvs/7a9f8b6c-cv.pdf",
  "personalPhotoUrl": "/uploads/images/photo.jpg"
}
```

**المخرجات — 200 OK:**
```json
{
  "id": 5,
  "userId": 2,
  "fullName": "الاسم الكامل على البطاقة",
  "organization": "قناة الجزيرة",
  "jobTitle": "مصور صحفي",
  "status": "Pending",
  "submittedAt": "2026-06-15T10:00:00Z"
}
```

---

### 4.2 طلبي الحالي — `GET /api/Accreditation/my-application`

**الصلاحية:** محمي | **الوصف:** يجلب طلب الاعتماد الخاص بالمستخدم الحالي.

**المخرجات — 200 OK:** نفس كائن الطلب مع حالته الحالية.

---

### 4.3 كل الطلبات (للإدارة) — `GET /api/Accreditation`

**الصلاحية:** Admin/Employee | **الفلاتر:** `?status=Pending` أو `Approved` أو `Rejected`

**المخرجات — 200 OK:** قائمة بجميع طلبات الاعتماد مفلترة حسب الحالة.

---

### 4.4 تفاصيل طلب — `GET /api/Accreditation/{id}`

**الصلاحية:** Admin/Employee

**المخرجات — 200 OK:** تفاصيل الطلب بالكامل مع بيانات المستخدم.

---

### 4.5 مراجعة طلب (قبول/رفض) — `PUT /api/Accreditation/{id}/review`

**الصلاحية:** Admin/Employee | **الوصف:** عند القبول يُولَّد رقم بطاقة فريد + QR Code تلقائياً.

**المدخلات (Body - JSON):**
```json
{
  "approve": true,
  "notes": "تم التحقق من بيانات الصحفي وتم القبول."
}
```

**المخرجات — 200 OK:**
```json
{
  "message": "Accreditation approved and Media Card generated successfully.",
  "cardNumber": "GACAM-MC-2026-48291",
  "expiryDate": "2027-06-15T00:00:00Z"
}
```

---

### 4.6 التحقق من بطاقة إعلامية — `GET /api/Accreditation/verify/card/{number}`

**الصلاحية:** عام | **الوصف:** يتحقق من صحة بطاقة الاعتماد عبر رقمها أو قراءة QR Code عليها.

**مثال:** `GET /api/Accreditation/verify/card/GACAM-MC-2026-48291`

**المخرجات في حال صالحة — 200 OK:**
```json
{
  "isValid": true,
  "cardNumber": "GACAM-MC-2026-48291",
  "holderName": "الاسم الكامل",
  "organization": "قناة الجزيرة",
  "jobTitle": "مصور صحفي",
  "expiryDate": "2027-06-15T00:00:00Z"
}
```

**المخرجات في حال غير صالحة:**
```json
{
  "isValid": false
}
```

---

## 5. إعدادات الموقع والشهادات (`/api/Settings`)

> التحكم الكامل في هوية الموقع (اللوجو، السوشيال ميديا، بيانات التواصل) وكذلك تصميم شهادات PDF المولدة.

---

### 5.1 جلب إعدادات الموقع — `GET /api/Settings`

**الصلاحية:** عام | **الوصف:** تُستخدم في الـ Header والـ Footer لعرض لوجو الموقع وبيانات التواصل والسوشيال ميديا.

**المخرجات — 200 OK:**
```json
{
  "id": 1,
  "siteTitleEn": "GACAM",
  "siteTitleAr": "الهيئة العامة للإعلام",
  "logoUrl": "/uploads/logos/logo.png",
  "socialLinksJson": "{\"facebook\":\"https://fb.com/gacam\",\"twitter\":\"https://twitter.com/gacam\",\"instagram\":\"https://instagram.com/gacam\",\"youtube\":\"\",\"linkedin\":\"\"}",
  "contactInfo": "{\"email\":\"info@gacam.media\",\"phone\":\"+1-416-123-4567\",\"address\":\"Toronto, Canada\"}"
}
```

> 💡 حقلا `socialLinksJson` و`contactInfo` هما نصوص JSON — قم بـ `JSON.parse()` عليهما في Angular لاستخدام القيم.

---

### 5.2 تعديل إعدادات الموقع — `PUT /api/Settings`

**الصلاحية:** Admin | **الوصف:** تعديل بيانات الهوية البصرية للموقع. اللوجو يُرفع أولاً عبر `/api/Files/upload/logos` ثم يُمرر هنا.

**المدخلات (Body - JSON):**
```json
{
  "id": 1,
  "siteTitleEn": "GACAM",
  "siteTitleAr": "الهيئة العامة للإعلام",
  "logoUrl": "/uploads/logos/new-logo.png",
  "socialLinksJson": "{\"facebook\":\"https://fb.com/gacam\",\"twitter\":\"https://twitter.com/gacam\",\"instagram\":\"\",\"youtube\":\"\",\"linkedin\":\"\"}",
  "contactInfo": "{\"email\":\"info@gacam.media\",\"phone\":\"+1-416-123-4567\",\"address\":\"Toronto, Canada\"}"
}
```

**المخرجات — 200 OK:** يعيد الكائن بعد التعديل.

---

### 5.3 جلب إعدادات تصميم الشهادات — `GET /api/Settings/certificate`

**الصلاحية:** Admin/Employee | **الوصف:** يعيد كل خصائص التصميم المستخدمة عند توليد ملفات PDF للشهادات.

**المخرجات — 200 OK:**
```json
{
  "id": 1,
  "primaryColor": "#003F4A",
  "secondaryColor": "#C9A96B",
  "borderColor": "#003F4A",
  "borderWidth": 10.0,
  "titleEn": "CERTIFICATE OF PARTICIPATION",
  "titleAr": "شهادة مشاركة تقديرية",
  "headerTextEn": "GULF & ARAB GENERAL COMMISSION FOR AUDIOVISUAL MEDIA",
  "headerTextAr": "الهيئة العامة للإعلام المرئي والمسموع والخليجي والعربي في كندا",
  "signatoryName": "Executive Director",
  "signatoryTitleEn": "GACAM Administration",
  "signatoryTitleAr": "إدارة الهيئة العامة للإعلام",
  "signatureImageUrl": null,
  "showLogo": true,
  "logoHeight": 60.0
}
```

---

### 5.4 تعديل إعدادات تصميم الشهادات — `PUT /api/Settings/certificate`

**الصلاحية:** Admin | **الوصف:** أي تعديل هنا يؤثر فوراً على الـ PDF المولَّد للشهادات الجديدة. صورة التوقيع تُرفع عبر `/api/Files/upload/logos` أولاً.

**المدخلات (Body - JSON):** نفس كائن الاستجابة أعلاه مع القيم الجديدة.

---

## 6. الأخبار والمقالات (`/api/News`)

> إدارة الأخبار، الإعلانات، والمقالات بنوعين: أخبار عامة أو بيانات صحفية.

---

### 6.1 جلب كل الأخبار — `GET /api/News`

**الصلاحية:** عام | **الفلاتر:** `?type=0` (أخبار) أو `?type=1` (بيانات صحفية)

**المخرجات — 200 OK:**
```json
[
  {
    "id": 1,
    "titleEn": "GACAM Launches New Training Program",
    "titleAr": "الهيئة تطلق برنامجاً تدريبياً جديداً",
    "contentEn": "Content in English...",
    "contentAr": "المحتوى بالعربية...",
    "imageUrl": "/uploads/news/news1.jpg",
    "publishedAt": "2026-06-01T09:00:00Z",
    "viewCount": 245,
    "type": "News"
  }
]
```

---

### 6.2 تفاصيل خبر — `GET /api/News/{id}`

**الصلاحية:** عام | **المخرجات:** نفس كائن الخبر بالتفصيل.

---

### 6.3 إحصاء مشاهدة — `POST /api/News/{id}/view`

**الصلاحية:** عام | **الوصف:** يزيد عداد المشاهدات بمقدار 1. يُستدعى عند فتح صفحة الخبر.

**المخرجات — 200 OK:**
```json
{ "message": "View count incremented." }
```

---

### 6.4 إنشاء خبر — `POST /api/News`

**الصلاحية:** Admin/Employee | **الوصف:** الصورة تُرفع أولاً عبر `/api/Files/upload/news`.

**المدخلات (Body - JSON):**
```json
{
  "titleEn": "New Event Announcement",
  "titleAr": "إعلان فعالية جديدة",
  "contentEn": "Full article content in English...",
  "contentAr": "المحتوى الكامل للمقال بالعربية...",
  "imageUrl": "/uploads/news/event.jpg",
  "type": 0
}
```

---

### 6.5 تعديل خبر — `PUT /api/News/{id}`

**الصلاحية:** Admin/Employee | **المدخلات:** نفس كائن الإنشاء.

---

### 6.6 حذف خبر — `DELETE /api/News/{id}`

**الصلاحية:** Admin/Employee | **المخرجات — 200 OK:**
```json
{ "message": "News article deleted successfully." }
```

---

## 7. الشركاء والداعمون (`/api/Partners`)

> إدارة شعارات وبيانات الشركاء الداعمين للهيئة.

---

### 7.1 جلب كل الشركاء — `GET /api/Partners`

**الصلاحية:** عام | **الفلاتر:** `?category=0` (ذهبي) أو `?category=1` (فضي) أو `?category=2` (عادي)

**المخرجات — 200 OK:**
```json
[
  {
    "id": 1,
    "nameEn": "Media Partner Co.",
    "nameAr": "شركة الشريك الإعلامي",
    "logoUrl": "/uploads/logos/partner1.png",
    "websiteUrl": "https://example.com",
    "category": "Gold"
  }
]
```

---

### 7.2 تفاصيل شريك — `GET /api/Partners/{id}`

**الصلاحية:** عام

---

### 7.3 إضافة شريك — `POST /api/Partners`

**الصلاحية:** Admin/Employee | **الوصف:** لوجو الشريك يُرفع أولاً عبر `/api/Files/upload/logos`.

**المدخلات (Body - JSON):**
```json
{
  "nameEn": "New Partner",
  "nameAr": "الشريك الجديد",
  "logoUrl": "/uploads/logos/partner.png",
  "websiteUrl": "https://partner.com",
  "category": 0
}
```

---

### 7.4 تعديل شريك — `PUT /api/Partners/{id}`

**الصلاحية:** Admin/Employee

---

### 7.5 حذف شريك — `DELETE /api/Partners/{id}`

**الصلاحية:** Admin/Employee

---

## 8. التطوع (`/api/Volunteers`)

> نظام استقبال وإدارة طلبات المتطوعين.

---

### 8.1 تقديم طلب تطوع — `POST /api/Volunteers`

**الصلاحية:** محمي | **الوصف:** الـ CV يُرفع أولاً عبر `/api/Files/upload/cvs`.

**المدخلات (Body - JSON):**
```json
{
  "fullName": "الاسم الكامل",
  "skills": "مونتاج فيديو، تصوير، إدارة سوشيال ميديا",
  "cvUrl": "/uploads/cvs/volunteer-cv.pdf",
  "notes": "خبرة 3 سنوات في الإنتاج الإعلامي"
}
```

**المخرجات — 200 OK:**
```json
{
  "id": 3,
  "userId": 2,
  "fullName": "الاسم الكامل",
  "status": "Pending",
  "submittedAt": "2026-06-15T10:00:00Z"
}
```

---

### 8.2 طلبي الحالي — `GET /api/Volunteers/my-application`

**الصلاحية:** محمي

---

### 8.3 كل الطلبات (للإدارة) — `GET /api/Volunteers`

**الصلاحية:** Admin/Employee | **الفلاتر:** `?status=Pending`

---

### 8.4 تفاصيل طلب — `GET /api/Volunteers/{id}`

**الصلاحية:** Admin/Employee

---

### 8.5 تحديث حالة الطلب — `PUT /api/Volunteers/{id}/status`

**الصلاحية:** Admin/Employee

**المدخلات (Body - JSON):**
```json
{
  "status": 1,
  "adminNotes": "تم قبول الطلب، سيتم التواصل قريباً."
}
```
> `status`: `0` = Pending, `1` = Approved, `2` = Rejected

---

## 9. الدورات التدريبية والتسجيل (`/api/Training`)

---

### 9.1 جلب كل الدورات — `GET /api/Training/courses`

**الصلاحية:** عام | **الفلاتر:** `?activeOnly=true`

**المخرجات — 200 OK:**
```json
[
  {
    "id": 1,
    "titleEn": "Professional Journalism",
    "titleAr": "الصحافة المهنية",
    "descriptionEn": "Advanced journalism course...",
    "descriptionAr": "دورة الصحافة المتقدمة...",
    "imageUrl": "/uploads/images/course1.jpg",
    "startDate": "2026-07-01T00:00:00Z",
    "endDate": "2026-07-30T00:00:00Z",
    "capacity": 30,
    "isActive": true
  }
]
```

---

### 9.2 تفاصيل دورة — `GET /api/Training/courses/{id}`

**الصلاحية:** عام

---

### 9.3 إنشاء دورة — `POST /api/Training/courses`

**الصلاحية:** Admin/Employee

**المدخلات (Body - JSON):**
```json
{
  "titleEn": "New Training Course",
  "titleAr": "دورة تدريبية جديدة",
  "descriptionEn": "Course description...",
  "descriptionAr": "وصف الدورة...",
  "imageUrl": "/uploads/images/course.jpg",
  "startDate": "2026-07-01T00:00:00Z",
  "endDate": "2026-07-30T00:00:00Z",
  "capacity": 25,
  "isActive": true
}
```

---

### 9.4 تعديل دورة — `PUT /api/Training/courses/{id}`

**الصلاحية:** Admin/Employee

---

### 9.5 حذف دورة — `DELETE /api/Training/courses/{id}`

**الصلاحية:** Admin/Employee

---

### 9.6 التسجيل في دورة — `POST /api/Training/enroll`

**الصلاحية:** محمي | **الوصف:** يسجل المستخدم في دورة. الحالة الأولية = `PendingPayment` حتى تُعتمد الدفعة.

**المدخلات (Body - JSON):**
```json
{
  "courseId": 1
}
```

**المخرجات — 200 OK:**
```json
{
  "id": 10,
  "courseId": 1,
  "userId": 2,
  "status": "PendingPayment",
  "enrolledAt": "2026-06-15T10:00:00Z"
}
```

---

### 9.7 تسجيلاتي — `GET /api/Training/my-enrollments`

**الصلاحية:** محمي

---

### 9.8 كل التسجيلات (للإدارة) — `GET /api/Training/enrollments`

**الصلاحية:** Admin/Employee | **الفلاتر:** `?status=PendingPayment`

---

### 9.9 تعديل حالة تسجيل — `PUT /api/Training/enrollments/{id}/status`

**الصلاحية:** Admin/Employee

**المدخلات (Body - JSON):**
```json
{
  "status": 2,
  "adminNotes": "تم التحقق من الدفع وتم القبول."
}
```
> `status`: `0` = PendingPayment, `1` = PendingApproval, `2` = Approved, `3` = Rejected

---

## 10. المدفوعات والتحويلات البنكية (`/api/Payments`)

> نظام إدارة إثباتات الدفع عبر التحويل البنكي.

---

### 10.1 رفع صورة إيصال الدفع — `POST /api/Payments/upload-receipt`

**الصلاحية:** محمي | **نوع الطلب:** `multipart/form-data`

**المدخلات (Form-Data):**
- `file` — صورة إيصال الدفع (JPG/PNG/PDF)

**المخرجات — 200 OK:**
```json
{
  "relativePath": "/uploads/receipts/receipt-abc.jpg",
  "absoluteUrl": "http://localhost:5244/uploads/receipts/receipt-abc.jpg"
}
```

---

### 10.2 إرسال إثبات دفع — `POST /api/Payments`

**الصلاحية:** محمي | **الوصف:** يرسل المستخدم بيانات حوالته البنكية بعد رفع الإيصال.

**المدخلات (Body - JSON):**
```json
{
  "amount": 150.00,
  "referenceNumber": "TXN-2026-98765",
  "receiptPhotoUrl": "/uploads/receipts/receipt-abc.jpg",
  "notes": "دفع رسوم دورة الصحافة المهنية"
}
```

**المخرجات — 200 OK:**
```json
{
  "id": 7,
  "userId": 2,
  "amount": 150.00,
  "referenceNumber": "TXN-2026-98765",
  "status": "Pending",
  "submittedAt": "2026-06-15T10:00:00Z"
}
```

---

### 10.3 مدفوعاتي — `GET /api/Payments/my-payments`

**الصلاحية:** محمي

---

### 10.4 كل المدفوعات (للإدارة) — `GET /api/Payments`

**الصلاحية:** Admin/Employee | **الفلاتر:** `?status=Pending`

---

### 10.5 تفاصيل دفعة — `GET /api/Payments/{id}`

**الصلاحية:** Admin/Employee

---

### 10.6 مراجعة ومعالجة دفعة — `PUT /api/Payments/{id}/review`

**الصلاحية:** Admin/Employee

**المدخلات (Body - JSON):**
```json
{
  "approve": true,
  "adminNotes": "تم استلام المبلغ وتنشيط الاشتراك."
}
```

---

## 11. الشهادات الإلكترونية (`/api/Certificates`)

> نظام توليد الشهادات كملفات PDF مع QR Code للتحقق.

---

### 11.1 طلب شهادة — `POST /api/Certificates`

**الصلاحية:** محمي | **الوصف:** يجب أن يكون المستخدم مقبولاً في الكورس أو كمتطوع قبل طلب الشهادة.

**المدخلات (Body - JSON):**
```json
{
  "fullNameOnCertificate": "الاسم كما يُطبع على الشهادة",
  "type": 0,
  "relatedRecordId": 1
}
```
> `type`: `0` = شهادة دورة تدريبية (Training), `1` = شهادة تطوع (Volunteer)
> `relatedRecordId`: رقم الدورة (CourseId) أو رقم طلب التطوع (VolunteerId)

**المخرجات — 200 OK:**
```json
{
  "id": 3,
  "certificateNumber": "GACAM-CERT-2026-54321",
  "fullNameOnCertificate": "الاسم كما يُطبع على الشهادة",
  "type": "Training",
  "issuedAt": "2026-06-15T11:00:00Z",
  "pdfUrl": "/uploads/certificates/cert_GACAM-CERT-2026-54321.pdf",
  "qrCodeData": "https://gacam.media/verify/certificate/GACAM-CERT-2026-54321"
}
```

---

### 11.2 شهاداتي — `GET /api/Certificates/my-certificates`

**الصلاحية:** محمي | **الوصف:** يعيد قائمة الشهادات الخاصة بالمستخدم الحالي.

---

### 11.3 كل الشهادات (للإدارة) — `GET /api/Certificates`

**الصلاحية:** Admin/Employee

---

### 11.4 التحقق من شهادة — `GET /api/Certificates/verify/{number}`

**الصلاحية:** عام | **الوصف:** الطريقة الرئيسية للتحقق سواء عبر إدخال رقم الشهادة يدوياً أو مسح الـ QR Code.

**آلية عمل QR Code:**
- عند إصدار الشهادة يُولَّد QR Code يحتوي الرابط: `https://gacam.media/verify/certificate/GACAM-CERT-2026-12345`
- عند مسح الـ QR Code يتم التوجيه لصفحة في الـ Frontend.
- الـ Frontend يستدعي هذه الواجهة بإرسال الرقم الكامل أو الرابط الكامل (الواجهة تستخلص الرقم تلقائياً).

**مثال الطلب:**
- `GET /api/Certificates/verify/GACAM-CERT-2026-54321`
- أو `GET /api/Certificates/verify/https://gacam.media/verify/certificate/GACAM-CERT-2026-54321`

**المخرجات — شهادة صحيحة (200 OK):**
```json
{
  "isValid": true,
  "certificateNumber": "GACAM-CERT-2026-54321",
  "fullNameOnCertificate": "الاسم المطبوع",
  "type": "Training",
  "relatedItemTitle": "Professional Journalism / الصحافة المهنية",
  "issuedAt": "2026-06-15T11:00:00Z"
}
```

**المخرجات — شهادة غير صحيحة:**
```json
{
  "isValid": false
}
```

---

### 11.5 تحميل شهادة — `GET /api/Certificates/download/{id}`

**الصلاحية:** محمي | **الوصف:** يعيد ملف PDF جاهز للتنزيل.

**المخرجات:** ملف PDF بالاسم `certificate_GACAM-CERT-2026-54321.pdf`

---

### 11.6 التحقق برفع ملف (PDF أو صورة) — `POST /api/Certificates/verify-file`

**الصلاحية:** عام | **نوع الطلب:** `multipart/form-data`

**الوصف:** يتحقق من صحة وصلاحية شهادة عبر رفع ملف الشهادة نفسه (سواء ملف PDF أو صورة PNG/JPG). يقوم النظام بقراءة كود الـ QR ميكانيكياً والتحقق من صحته.

**المدخلات (Form-Data):**
- `file` — ملف الشهادة (PDF أو صورة PNG/JPG/JPEG/BMP)

**المخرجات — شهادة صالحة (200 OK):**
```json
{
  "isValid": true,
  "certificateNumber": "GACAM-CERT-2026-54321",
  "fullNameOnCertificate": "الاسم المطبوع على الشهادة",
  "type": "Training",
  "relatedItemTitle": "Professional Journalism / الصحافة المهنية",
  "issuedAt": "2026-06-15T11:00:00Z"
}
```

**المخرجات — شهادة غير صالحة أو تعذر قراءة الكود:**
```json
{
  "isValid": false
}
```

---


## 12. الرسوم والخدمات (`/api/ServiceFees`)

> جدول أسعار الخدمات المختلفة (رسوم الاعتماد، التدريب، العضوية).

---

### 12.1 جلب كل الرسوم — `GET /api/ServiceFees`

**الصلاحية:** عام

**المخرجات — 200 OK:**
```json
[
  {
    "code": "ACCREDITATION",
    "nameEn": "Media Accreditation Fee",
    "nameAr": "رسوم الاعتماد الإعلامي",
    "amount": 200.00,
    "currency": "CAD"
  },
  {
    "code": "TRAINING_BASIC",
    "nameEn": "Basic Training Course",
    "nameAr": "الدورة التدريبية الأساسية",
    "amount": 150.00,
    "currency": "CAD"
  }
]
```

---

### 12.2 جلب رسم بالكود — `GET /api/ServiceFees/{code}`

**الصلاحية:** عام | **مثال:** `GET /api/ServiceFees/ACCREDITATION`

---

### 12.3 تعديل رسم — `PUT /api/ServiceFees/{code}`

**الصلاحية:** Admin

**المدخلات (Body - JSON):**
```json
{
  "nameEn": "Media Accreditation Fee",
  "nameAr": "رسوم الاعتماد الإعلامي",
  "amount": 250.00,
  "currency": "CAD"
}
```

---

## 13. التقارير وتصدير البيانات (`/api/Reports`)

> تصدير بيانات النظام كملفات Excel للاستخدام الإداري.

> **الصلاحية لكل الواجهات:** Admin/Employee

---

### 13.1 تصدير تقرير المدفوعات — `GET /api/Reports/payments`

**الوصف:** يصدر جدول بيانات Excel يحتوي جميع سجلات المدفوعات مع تفاصيلها.

**المخرجات:** ملف `.xlsx` بالاسم `GACAM_Payments_20260615_110000.xlsx`

---

### 13.2 تصدير سجل العمليات — `GET /api/Reports/auditlogs`

**الوصف:** يصدر ملف Excel يحتوي كل عمليات الإضافة والتعديل والحذف في النظام.

**المخرجات:** ملف `.xlsx` بالاسم `GACAM_AuditLogs_20260615_110000.xlsx`

---

### 13.3 تصدير قائمة المستخدمين — `GET /api/Reports/users`

**الوصف:** يصدر جدول Excel بجميع المستخدمين المسجلين وصلاحياتهم.

**المخرجات:** ملف `.xlsx` بالاسم `GACAM_Users_20260615_110000.xlsx`

---

## 14. سجل العمليات (`/api/AuditLogs`)

---

### 14.1 جلب كل السجلات — `GET /api/AuditLogs`

**الصلاحية:** Admin | **الوصف:** يعيد سجل كامل بكل العمليات التي تمت في النظام (من قام بها، ومتى، وعلى أي بيانات).

**المخرجات — 200 OK:**
```json
[
  {
    "id": 1,
    "userId": 1,
    "action": "UPDATE",
    "entityName": "Pages",
    "entityId": "3",
    "details": "Updated page content for slug: about-us",
    "createdAt": "2026-06-15T11:30:00Z"
  }
]
```

---

## ملحق: قيم Enums

| الـ Enum | القيم |
|---|---|
| **ApplicationStatus** | `0` = Pending, `1` = Approved, `2` = Rejected |
| **EnrollmentStatus** | `0` = PendingPayment, `1` = PendingApproval, `2` = Approved, `3` = Rejected |
| **PaymentStatus** | `0` = Pending, `1` = Approved, `2` = Rejected |
| **CertificateType** | `0` = Training, `1` = Volunteer |
| **NewsType** | `0` = News, `1` = PressRelease |
| **PartnerCategory** | `0` = Gold, `1` = Silver, `2` = Regular |

---

## ملحق: الحساب الافتراضي للإدارة

| البيان | القيمة |
|---|---|
| **البريد الإلكتروني** | `admin@gacam.media` |
| **كلمة المرور** | `Admin@Gacam2026` |
| **الدور** | `Admin` |

> ⚠️ يُنصح بتغيير كلمة المرور فور التشغيل الأول في بيئة الإنتاج.
