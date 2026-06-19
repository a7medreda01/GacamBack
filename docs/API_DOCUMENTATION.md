# GACAM Backend API Documentation

**Project:** TProjectGacam  
**API Title:** GACAM Backend API v1  
**Base URL:** `https://{host}/api`  
**Swagger UI (Development):** `/swagger`

Gulf & Arab General Commission for Audiovisual Media — REST API for CMS, training, volunteers, payments, certificates, and media accreditation.

---

## Table of Contents

1. [Authentication](#authentication)
2. [Common Conventions](#common-conventions)
3. [Endpoints](#endpoints)
   - [Auth](#auth-apiauth)
   - [Pages](#pages-apipages)
   - [News](#news-apinews)
   - [Partners](#partners-apipartners)
   - [Volunteers](#volunteers-apivolunteers)
   - [Training](#training-apitraining)
   - [Service Fees](#service-fees-apiservicefees)
   - [Payments](#payments-apipayments)
   - [Certificates](#certificates-apicertificates)
   - [Accreditation](#accreditation-apiaccreditation)
   - [Settings](#settings-apisettings)
   - [Files](#files-apifiles)
   - [Audit Logs](#audit-logs-apiauditlogs)
   - [Reports](#reports-apireports)
4. [DTOs Reference](#dtos-reference)
5. [Entities Reference](#entities-reference)
6. [Enums Reference](#enums-reference)

---

## Authentication

The API uses **JWT Bearer** authentication.

### Login

```http
POST /api/Auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "yourPassword"
}
```

**Response:** `LoginResponse` — includes `token` and `user`.

### Using the Token

Include the JWT in every protected request:

```http
Authorization: Bearer {token}
```

### JWT Configuration

| Setting | Value |
|---------|-------|
| Scheme | Bearer JWT |
| Issuer | `gacam.media` (config: `JwtSettings:Issuer`) |
| Audience | `gacam-client` (config: `JwtSettings:Audience`) |
| Key | `JwtSettings:Key` in `appsettings.json` |

### Roles

| Role | Description |
|------|-------------|
| `Admin` | Full system access |
| `Employee` | CMS and operational access |
| *(authenticated)* | Any logged-in user without a specific role requirement |

---

## Common Conventions

| Topic | Details |
|-------|---------|
| Content-Type | `application/json` unless noted (`multipart/form-data` for uploads) |
| Date/Time | ISO 8601 (`DateTime`) |
| Null fields | Omitted from JSON responses when null |
| Error responses | `{ "message": "..." }` or model validation errors |
| File uploads | Max **10 MB** (Files controller); stored under `wwwroot/uploads/` |
| CORS | `http://localhost:4200` (Development) |

---

## Endpoints

### Auth (`/api/Auth`)

| Method | Route | Auth | Request | Response | Description |
|--------|-------|------|---------|----------|-------------|
| `POST` | `/register` | Public | `RegisterRequest` | `UserDto` | Register a new user |
| `POST` | `/login` | Public | `LoginRequest` | `LoginResponse` | Authenticate and receive JWT |
| `GET` | `/profile` | Bearer | — | `UserDto` | Get current user profile |
| `PUT` | `/profile` | Bearer | `UpdateProfileRequest` | `UserDto` | Update current user profile |
| `POST` | `/change-password` | Bearer | `ChangePasswordRequest` | `{ message }` | Change password |
| `GET` | `/users` | Admin, Employee | — | `UserDto[]` | List all users |
| `POST` | `/users/{id}/roles` | Admin | `AssignRoleRequest` | `{ message }` | Assign role to user |
| `DELETE` | `/users/{id}/roles` | Admin | Query: `roleName` | `{ message }` | Remove role from user |

---

### Pages (`/api/Pages`)

| Method | Route | Auth | Request | Response | Description |
|--------|-------|------|---------|----------|-------------|
| `GET` | `/` | Public | — | `PageDto[]` | List all CMS pages |
| `GET` | `/{slug}` | Public | Path: `slug` | `PageDto` | Get page by slug |
| `PUT` | `/{slug}` | Admin, Employee | `PageUpdateRequest` | `PageDto` | Update page content |

---

### News (`/api/News`)

| Method | Route | Auth | Request | Response | Description |
|--------|-------|------|---------|----------|-------------|
| `GET` | `/` | Public | Query: `type?` (`NewsType`) | `NewsDto[]` | List news (optional filter) |
| `GET` | `/{id}` | Public | Path: `id` | `NewsDto` | Get news by ID |
| `POST` | `/{id}/view` | Public | Path: `id` | `{ message }` | Increment view count |
| `POST` | `/` | Admin, Employee | `NewsCreateRequest` | `NewsDto` (201) | Create news |
| `PUT` | `/{id}` | Admin, Employee | `NewsUpdateRequest` | `NewsDto` | Update news |
| `DELETE` | `/{id}` | Admin, Employee | Path: `id` | `{ message }` | Delete news |

---

### Partners (`/api/Partners`)

| Method | Route | Auth | Request | Response | Description |
|--------|-------|------|---------|----------|-------------|
| `GET` | `/` | Public | Query: `category?` (`PartnerCategory`) | `PartnerDto[]` | List partners |
| `GET` | `/{id}` | Public | Path: `id` | `PartnerDto` | Get partner by ID |
| `POST` | `/` | Admin, Employee | `PartnerCreateRequest` | `PartnerDto` (201) | Create partner |
| `PUT` | `/{id}` | Admin, Employee | `PartnerUpdateRequest` | `PartnerDto` | Update partner |
| `DELETE` | `/{id}` | Admin, Employee | Path: `id` | `{ message }` | Delete partner |

---

### Volunteers (`/api/Volunteers`)

| Method | Route | Auth | Request | Response | Description |
|--------|-------|------|---------|----------|-------------|
| `POST` | `/` | Bearer | `VolunteerRegisterRequest` | `VolunteerDto` | Submit volunteer application |
| `GET` | `/my-application` | Bearer | — | `VolunteerDto` | Get current user's application |
| `GET` | `/` | Admin, Employee | Query: `status?` (`ApplicationStatus`) | `VolunteerDto[]` | List all applications |
| `GET` | `/{id}` | Admin, Employee | Path: `id` | `VolunteerDto` | Get application by ID |
| `PUT` | `/{id}/status` | Admin, Employee | `VolunteerStatusUpdateRequest` | `VolunteerDto` | Update application status |

---

### Training (`/api/Training`)

| Method | Route | Auth | Request | Response | Description |
|--------|-------|------|---------|----------|-------------|
| `GET` | `/courses` | Public | Query: `activeOnly?` (bool) | `CourseDto[]` | List courses |
| `GET` | `/courses/{id}` | Public | Path: `id` | `CourseDto` | Get course by ID |
| `POST` | `/courses` | Admin, Employee | `CourseCreateRequest` | `CourseDto` (201) | Create course |
| `PUT` | `/courses/{id}` | Admin, Employee | `CourseUpdateRequest` | `CourseDto` | Update course |
| `DELETE` | `/courses/{id}` | Admin, Employee | Path: `id` | `{ message }` | Delete course |
| `POST` | `/enroll` | Bearer | `EnrollmentRequest` | `CourseEnrollmentDto` | Enroll in a course |
| `GET` | `/my-enrollments` | Bearer | — | `CourseEnrollmentDto[]` | Current user's enrollments |
| `GET` | `/enrollments` | Admin, Employee | Query: `status?` (`EnrollmentStatus`) | `CourseEnrollmentDto[]` | List all enrollments |
| `PUT` | `/enrollments/{id}/status` | Admin, Employee | `EnrollmentStatusRequest` | `CourseEnrollmentDto` | Update enrollment status |

---

### Service Fees (`/api/ServiceFees`)

| Method | Route | Auth | Request | Response | Description |
|--------|-------|------|---------|----------|-------------|
| `GET` | `/` | Public | — | `ServiceFeeDto[]` | List all service fees |
| `GET` | `/{code}` | Public | Path: `code` | `ServiceFeeDto` | Get fee by code |
| `PUT` | `/{code}` | Admin | `ServiceFeeUpdateRequest` | `ServiceFeeDto` | Update service fee |

---

### Payments (`/api/Payments`)

| Method | Route | Auth | Request | Response | Description |
|--------|-------|------|---------|----------|-------------|
| `POST` | `/` | Bearer | `PaymentSubmitRequest` | `PaymentDto` | Submit a payment |
| `POST` | `/upload-receipt` | Bearer | Form: `file` | `{ relativePath, absoluteUrl }` | Upload payment receipt |
| `GET` | `/my-payments` | Bearer | — | `PaymentDto[]` | Current user's payments |
| `GET` | `/` | Admin, Employee | Query: `status?` (`PaymentStatus`) | `PaymentDto[]` | List all payments |
| `GET` | `/{id}` | Admin, Employee | Path: `id` | `PaymentDto` | Get payment by ID |
| `PUT` | `/{id}/review` | Admin, Employee | `PaymentReviewRequest` | `PaymentDto` | Review / approve payment |

---

### Certificates (`/api/Certificates`)

| Method | Route | Auth | Request | Response | Description |
|--------|-------|------|---------|----------|-------------|
| `POST` | `/` | Bearer | `CertificateRequestDto` | `CertificateDto` | Request a certificate |
| `GET` | `/my-certificates` | Bearer | — | `CertificateDto[]` | Current user's certificates |
| `GET` | `/` | Admin, Employee | — | `CertificateDto[]` | List all certificates |
| `GET` | `/verify/{number}` | Public | Path: `number` | `CertificateVerifyDto` | Verify by certificate number |
| `POST` | `/verify-file` | Public | Form: `file` (PDF/image) | `CertificateVerifyDto` | Verify by uploading certificate file (QR scan) |
| `GET` | `/download/{id}` | Public | Path: `id` | PDF file (`application/pdf`) | Download certificate PDF |

---

### Accreditation (`/api/Accreditation`)

| Method | Route | Auth | Request | Response | Description |
|--------|-------|------|---------|----------|-------------|
| `POST` | `/apply` | Bearer | Form: `AccreditationApplyRequest` | `AccreditationDto` | Submit media accreditation |
| `GET` | `/my-application` | Bearer | — | `AccreditationDto` | Current user's application |
| `GET` | `/` | Admin, Employee | Query: `status?` (`ApplicationStatus`) | `AccreditationDto[]` | List all applications |
| `GET` | `/{id}` | Admin, Employee | Path: `id` | `AccreditationDto` | Get application by ID |
| `PUT` | `/{id}/review` | Admin, Employee | `AccreditationReviewRequest` | `AccreditationDto` | Review application |
| `GET` | `/verify/card/{number}` | Public | Path: `number` | `CardVerifyDto` | Verify media card by number |

---

### Settings (`/api/Settings`)

| Method | Route | Auth | Request | Response | Description |
|--------|-------|------|---------|----------|-------------|
| `GET` | `/` | Public | — | `Setting` | Get website settings |
| `PUT` | `/` | Admin | `SettingUpdateRequest` | `Setting` | Update website settings |
| `POST` | `/upload-logo` | Admin | Form: `file` | `{ relativePath, absoluteUrl }` | Upload site logo (auto-saved) |
| `GET` | `/certificate` | Admin, Employee | — | `CertificateDesign` | Get certificate design config |
| `PUT` | `/certificate` | Admin | `CertificateDesignUpdateRequest` | `CertificateDesign` | Update certificate design |
| `POST` | `/certificate/upload-signature` | Admin | Form: `file` | `{ relativePath, absoluteUrl }` | Upload signature image |
| `POST` | `/certificate/upload-background` | Admin | Form: `file` | `{ relativePath, absoluteUrl }` | Upload certificate background |
| `DELETE` | `/certificate/background` | Admin | — | `{ message }` | Remove certificate background |

#### Inline Request DTOs (SettingsController)

**SettingUpdateRequest**

| Property | Type | Required |
|----------|------|----------|
| `id` | int | Yes |
| `siteTitleEn` | string | Yes |
| `siteTitleAr` | string | Yes |
| `logoUrl` | string | Yes |
| `socialLinksJson` | string | Yes (JSON, default `{}`) |
| `contactInfo` | string | Yes (JSON, default `{}`) |

**CertificateDesignUpdateRequest**

| Property | Type | Required |
|----------|------|----------|
| `id` | int | Yes |
| `primaryColor` | string | Yes (default `#003F4A`) |
| `secondaryColor` | string | Yes (default `#C9A96B`) |
| `borderColor` | string | Yes |
| `borderWidth` | float | Yes |
| `titleEn` | string | Yes |
| `titleAr` | string | Yes |
| `headerTextEn` | string | Yes |
| `headerTextAr` | string | Yes |
| `signatoryName` | string? | No |
| `signatoryTitleEn` | string? | No |
| `signatoryTitleAr` | string? | No |
| `signatureImageUrl` | string? | No |
| `backgroundImageUrl` | string? | No |
| `showLogo` | bool | Yes |
| `logoHeight` | float | Yes |

---

### Files (`/api/Files`)

All endpoints require **Bearer** authentication.

| Method | Route | Auth | Request | Response | Description |
|--------|-------|------|---------|----------|-------------|
| `POST` | `/upload/{folder}` | Bearer | Form: `file` | `{ relativePath, absoluteUrl, fileName }` | Upload file to subfolder |

**Allowed folders:** `images`, `cvs`, `receipts`, `logos`, `documents`, `news`

---

### Audit Logs (`/api/AuditLogs`)

All endpoints require **Admin** role.

| Method | Route | Auth | Request | Response | Description |
|--------|-------|------|---------|----------|-------------|
| `GET` | `/` | Admin | — | `AuditLogDto[]` | List all audit logs |

---

### Reports (`/api/Reports`)

All endpoints require **Admin** or **Employee** role. Returns Excel (`.xlsx`) files.

| Method | Route | Auth | Response | Description |
|--------|-------|------|----------|-------------|
| `GET` | `/payments` | Admin, Employee | Excel file | Export payments report |
| `GET` | `/auditlogs` | Admin, Employee | Excel file | Export audit logs report |
| `GET` | `/users` | Admin, Employee | Excel file | Export users report |

---

## DTOs Reference

All DTOs live in `AppBL/DTOs/`. Enums are defined in `AppDAL.Entities`.

### AuthDTOs

#### RegisterRequest
| Property | Type | Validation |
|----------|------|------------|
| `email` | string | Required, Email, MaxLength(150) |
| `password` | string | Required, MinLength(6), MaxLength(100) |
| `fullName` | string | Required, MaxLength(150) |
| `phoneNumber` | string? | MaxLength(20) |

#### LoginRequest
| Property | Type | Validation |
|----------|------|------------|
| `email` | string | Required, Email |
| `password` | string | Required |

#### LoginResponse
| Property | Type |
|----------|------|
| `token` | string |
| `user` | UserDto |

#### UserDto
| Property | Type |
|----------|------|
| `id` | int |
| `email` | string |
| `fullName` | string |
| `phoneNumber` | string? |
| `isActive` | bool |
| `createdAt` | DateTime |
| `roles` | List\<string\> |

#### UpdateProfileRequest
| Property | Type | Validation |
|----------|------|------------|
| `fullName` | string | Required, MaxLength(150) |
| `phoneNumber` | string? | MaxLength(20) |

#### ChangePasswordRequest
| Property | Type | Validation |
|----------|------|------------|
| `currentPassword` | string | Required |
| `newPassword` | string | Required, MinLength(6), MaxLength(100) |

#### AssignRoleRequest
| Property | Type | Validation |
|----------|------|------------|
| `roleName` | string | Required |

---

### PageDTOs

#### PageDto
| Property | Type |
|----------|------|
| `id` | int |
| `slug` | string |
| `titleEn` | string |
| `titleAr` | string |
| `contentEn` | string |
| `contentAr` | string |
| `imageUrl` | string? |
| `updatedAt` | DateTime |
| `updatedByUserName` | string? |

#### PageUpdateRequest
| Property | Type | Validation |
|----------|------|------------|
| `titleEn` | string | Required, MaxLength(200) |
| `titleAr` | string | Required, MaxLength(200) |
| `contentEn` | string | Required |
| `contentAr` | string | Required |
| `imageUrl` | string? | — |

---

### NewsDTOs

#### NewsDto
| Property | Type |
|----------|------|
| `id` | int |
| `type` | NewsType |
| `titleEn` | string |
| `titleAr` | string |
| `contentEn` | string |
| `contentAr` | string |
| `imageUrl` | string? |
| `publishedAt` | DateTime |
| `viewCount` | int |
| `isActive` | bool |

#### NewsCreateRequest / NewsUpdateRequest
| Property | Type | Validation |
|----------|------|------------|
| `type` | NewsType | Required |
| `titleEn` | string | Required, MaxLength(250) |
| `titleAr` | string | Required, MaxLength(250) |
| `contentEn` | string | Required |
| `contentAr` | string | Required |
| `imageUrl` | string? | — |
| `isActive` | bool | Create: default `true` |

---

### PartnerDTOs

#### PartnerDto
| Property | Type |
|----------|------|
| `id` | int |
| `nameEn` | string |
| `nameAr` | string |
| `logoUrl` | string |
| `websiteUrl` | string? |
| `category` | PartnerCategory |
| `displayOrder` | int |
| `isActive` | bool |
| `createdAt` | DateTime |

#### PartnerCreateRequest
| Property | Type | Validation |
|----------|------|------------|
| `nameEn` | string | Required, MaxLength(150) |
| `nameAr` | string | Required, MaxLength(150) |
| `logoUrl` | string | Required, MaxLength(500) |
| `websiteUrl` | string? | MaxLength(500) |
| `category` | PartnerCategory | Required |
| `displayOrder` | int | default `0` |
| `isActive` | bool | default `true` |

#### PartnerUpdateRequest
Same fields as `PartnerCreateRequest` (all required except `websiteUrl`).

---

### VolunteerDTOs

#### VolunteerDto
| Property | Type |
|----------|------|
| `id` | int |
| `userId` | int |
| `fullName` | string |
| `email` | string |
| `phone` | string |
| `cvUrl` | string |
| `skills` | string? |
| `area` | VolunteeringArea |
| `status` | ApplicationStatus |
| `appliedAt` | DateTime |

#### VolunteerRegisterRequest
| Property | Type | Validation |
|----------|------|------------|
| `fullName` | string | Required, MaxLength(150) |
| `email` | string | Required, Email, MaxLength(150) |
| `phone` | string | Required, MaxLength(20) |
| `cvUrl` | string | Required, MaxLength(500) |
| `skills` | string? | MaxLength(500) |
| `area` | VolunteeringArea | Required |

#### VolunteerStatusUpdateRequest
| Property | Type | Validation |
|----------|------|------------|
| `status` | ApplicationStatus | Required |

---

### TrainingDTOs

#### CourseDto
| Property | Type |
|----------|------|
| `id` | int |
| `titleEn` | string |
| `titleAr` | string |
| `descriptionEn` | string |
| `descriptionAr` | string |
| `feeAmount` | decimal |
| `startDate` | DateTime |
| `endDate` | DateTime |
| `isActive` | bool |
| `createdAt` | DateTime |

#### CourseCreateRequest / CourseUpdateRequest
| Property | Type | Validation |
|----------|------|------------|
| `titleEn` | string | Required, MaxLength(200) |
| `titleAr` | string | Required, MaxLength(200) |
| `descriptionEn` | string | Required |
| `descriptionAr` | string | Required |
| `feeAmount` | decimal | Range(0, 100000) |
| `startDate` | DateTime | Required |
| `endDate` | DateTime | Required |
| `isActive` | bool | Create: default `true` |

#### CourseEnrollmentDto
| Property | Type |
|----------|------|
| `id` | int |
| `courseId` | int |
| `courseTitleEn` | string |
| `courseTitleAr` | string |
| `userId` | int |
| `userFullName` | string |
| `userEmail` | string |
| `status` | EnrollmentStatus |
| `paymentId` | int? |
| `createdAt` | DateTime |

#### EnrollmentRequest
| Property | Type | Validation |
|----------|------|------------|
| `courseId` | int | Required |

#### EnrollmentStatusRequest
| Property | Type | Validation |
|----------|------|------------|
| `status` | EnrollmentStatus | Required |

---

### ServiceFeeDTOs

#### ServiceFeeDto
| Property | Type |
|----------|------|
| `id` | int |
| `code` | string |
| `nameEn` | string |
| `nameAr` | string |
| `processingFee` | decimal |
| `shippingFee` | decimal |
| `isActive` | bool |
| `updatedAt` | DateTime |

#### ServiceFeeUpdateRequest
| Property | Type | Validation |
|----------|------|------------|
| `processingFee` | decimal | Range(0, 10000) |
| `shippingFee` | decimal | Range(0, 10000) |
| `isActive` | bool | — |

---

### PaymentDTOs

#### PaymentDto
| Property | Type |
|----------|------|
| `id` | int |
| `userId` | int |
| `userFullName` | string |
| `userEmail` | string |
| `amount` | decimal |
| `senderName` | string |
| `referenceNumber` | string |
| `receiptUrl` | string |
| `type` | PaymentType |
| `relatedRecordId` | int |
| `status` | PaymentStatus |
| `createdAt` | DateTime |
| `verifiedAt` | DateTime? |
| `verifiedByUserName` | string? |
| `adminNotes` | string? |

#### PaymentSubmitRequest
| Property | Type | Validation |
|----------|------|------------|
| `amount` | decimal | Required, Range(1, 100000) |
| `senderName` | string | Required, MaxLength(150) |
| `referenceNumber` | string | Required, MaxLength(100) |
| `receiptUrl` | string | Required, MaxLength(500) |
| `type` | PaymentType | Required |
| `relatedRecordId` | int | Required |

#### PaymentReviewRequest
| Property | Type | Validation |
|----------|------|------------|
| `status` | PaymentStatus | Required |
| `adminNotes` | string? | MaxLength(500) |

---

### CertificateDTOs

#### CertificateDto
| Property | Type |
|----------|------|
| `id` | int |
| `userId` | int |
| `userFullName` | string |
| `type` | CertificateType |
| `relatedRecordId` | int? |
| `fullNameOnCertificate` | string |
| `certificateNumber` | string |
| `qrCodeData` | string |
| `pdfUrl` | string |
| `issuedAt` | DateTime |

#### CertificateRequestDto
| Property | Type | Validation |
|----------|------|------------|
| `type` | CertificateType | Required |
| `relatedRecordId` | int? | CourseId or VolunteerId |
| `fullNameOnCertificate` | string | Required, MaxLength(200) |
| `requestPrinted` | bool | default `false` |

#### CertificateVerifyDto
| Property | Type |
|----------|------|
| `isValid` | bool |
| `certificateNumber` | string? |
| `fullNameOnCertificate` | string? |
| `type` | string? |
| `relatedItemTitle` | string? |
| `issuedAt` | DateTime? |

---

### AccreditationDTOs

#### AccreditationDto
| Property | Type |
|----------|------|
| `id` | int |
| `userId` | int |
| `userFullName` | string |
| `userEmail` | string |
| `category` | AccreditationCategory |
| `status` | ApplicationStatus |
| `documentUrl` | string |
| `createdAt` | DateTime |
| `checkedAt` | DateTime? |
| `checkedByUserName` | string? |
| `mediaCard` | MediaCardDto? |

#### MediaCardDto
| Property | Type |
|----------|------|
| `id` | int |
| `cardNumber` | string |
| `qrCodeData` | string |
| `status` | CardStatus |
| `issuedAt` | DateTime |
| `expiresAt` | DateTime |

#### AccreditationApplyRequest *(multipart/form-data)*
| Property | Type | Validation |
|----------|------|------------|
| `category` | AccreditationCategory | Required |
| `document` | IFormFile | Required |

#### AccreditationReviewRequest
| Property | Type | Validation |
|----------|------|------------|
| `status` | ApplicationStatus | Required |

#### CardVerifyDto
| Property | Type |
|----------|------|
| `isValid` | bool |
| `cardNumber` | string? |
| `fullName` | string? |
| `category` | string? |
| `status` | string? |
| `issuedAt` | DateTime? |
| `expiresAt` | DateTime? |

---

### AuditLogDTOs

#### AuditLogDto
| Property | Type |
|----------|------|
| `id` | int |
| `userId` | int? |
| `userEmail` | string? |
| `action` | string |
| `tableName` | string |
| `recordId` | string? |
| `oldValues` | string? |
| `newValues` | string? |
| `timestamp` | DateTime |
| `ipAddress` | string? |

---

### FileUploadResultDto

#### FileUploadResultDto
| Property | Type |
|----------|------|
| `relativePath` | string |
| `absoluteUrl` | string |
| `fileName` | string |

---

## Entities Reference

Database entities in `AppDAL/Entities/`. Returned directly by some endpoints (e.g. Settings).

### User
| Property | Type | Notes |
|----------|------|-------|
| `id` | int | PK |
| `email` | string | Required, MaxLength(150) |
| `passwordHash` | string | Required, MaxLength(256) — never returned in API |
| `fullName` | string | Required, MaxLength(150) |
| `phoneNumber` | string? | MaxLength(20) |
| `createdAt` | DateTime | |
| `isActive` | bool | default `true` |
| `userRoles` | ICollection\<UserRole\> | Navigation |

### Role
| Property | Type | Notes |
|----------|------|-------|
| `id` | int | PK |
| `name` | string | Required, MaxLength(50) |
| `userRoles` | ICollection\<UserRole\> | Navigation |

### UserRole
| Property | Type | Notes |
|----------|------|-------|
| `id` | int | PK |
| `userId` | int | FK → User |
| `roleId` | int | FK → Role |

### Page
| Property | Type | Notes |
|----------|------|-------|
| `id` | int | PK |
| `slug` | string | Required, MaxLength(100) |
| `titleEn` | string | Required, MaxLength(200) |
| `titleAr` | string | Required, MaxLength(200) |
| `contentEn` | string | Required |
| `contentAr` | string | Required |
| `imageUrl` | string? | MaxLength(500) |
| `updatedAt` | DateTime | |
| `updatedByUserId` | int? | FK → User |

### News
| Property | Type | Notes |
|----------|------|-------|
| `id` | int | PK |
| `type` | NewsType | Required |
| `titleEn` | string | Required, MaxLength(250) |
| `titleAr` | string | Required, MaxLength(250) |
| `contentEn` | string | Required |
| `contentAr` | string | Required |
| `imageUrl` | string? | MaxLength(500) |
| `publishedAt` | DateTime | |
| `viewCount` | int | default `0` |
| `isActive` | bool | default `true` |

### Partner
| Property | Type | Notes |
|----------|------|-------|
| `id` | int | PK |
| `nameEn` | string | Required, MaxLength(150) |
| `nameAr` | string | Required, MaxLength(150) |
| `logoUrl` | string | Required, MaxLength(500) |
| `websiteUrl` | string? | MaxLength(500) |
| `category` | PartnerCategory | Required |
| `displayOrder` | int | default `0` |
| `isActive` | bool | default `true` |
| `createdAt` | DateTime | |

### Volunteer
| Property | Type | Notes |
|----------|------|-------|
| `id` | int | PK |
| `userId` | int | FK → User |
| `fullName` | string | Required, MaxLength(150) |
| `email` | string | Required, MaxLength(150) |
| `phone` | string | Required, MaxLength(20) |
| `cvUrl` | string | Required, MaxLength(500) |
| `skills` | string? | MaxLength(500) |
| `area` | VolunteeringArea | Required |
| `status` | ApplicationStatus | default `Pending` |
| `appliedAt` | DateTime | |

### Course
| Property | Type | Notes |
|----------|------|-------|
| `id` | int | PK |
| `titleEn` | string | Required, MaxLength(200) |
| `titleAr` | string | Required, MaxLength(200) |
| `descriptionEn` | string | Required |
| `descriptionAr` | string | Required |
| `feeAmount` | decimal(18,2) | |
| `startDate` | DateTime | |
| `endDate` | DateTime | |
| `isActive` | bool | default `true` |
| `createdAt` | DateTime | |
| `enrollments` | ICollection\<CourseEnrollment\> | Navigation |

### CourseEnrollment
| Property | Type | Notes |
|----------|------|-------|
| `id` | int | PK |
| `courseId` | int | FK → Course |
| `userId` | int | FK → User |
| `status` | EnrollmentStatus | default `Pending` |
| `paymentId` | int? | FK → Payment |
| `createdAt` | DateTime | |

### ServiceFee
| Property | Type | Notes |
|----------|------|-------|
| `id` | int | PK |
| `code` | string | Required, MaxLength(100) |
| `nameEn` | string | Required, MaxLength(150) |
| `nameAr` | string | Required, MaxLength(150) |
| `processingFee` | decimal(18,2) | |
| `shippingFee` | decimal(18,2) | |
| `isActive` | bool | default `true` |
| `updatedAt` | DateTime | |

### Payment
| Property | Type | Notes |
|----------|------|-------|
| `id` | int | PK |
| `userId` | int | FK → User |
| `amount` | decimal(18,2) | |
| `senderName` | string | Required, MaxLength(150) |
| `referenceNumber` | string | Required, MaxLength(100) |
| `receiptUrl` | string | Required, MaxLength(500) |
| `type` | PaymentType | Required |
| `relatedRecordId` | int | Required |
| `status` | PaymentStatus | default `PendingVerification` |
| `createdAt` | DateTime | |
| `verifiedAt` | DateTime? | |
| `verifiedByUserId` | int? | FK → User |
| `adminNotes` | string? | MaxLength(500) |

### Certificate
| Property | Type | Notes |
|----------|------|-------|
| `id` | int | PK |
| `userId` | int | FK → User |
| `type` | CertificateType | Required |
| `relatedRecordId` | int? | CourseId / VolunteerId |
| `fullNameOnCertificate` | string | Required, MaxLength(200) |
| `certificateNumber` | string | Required, MaxLength(100), unique |
| `qrCodeData` | string | Required, MaxLength(500) |
| `pdfUrl` | string | Required, MaxLength(500) |
| `issuedAt` | DateTime | |

### CertificateDesign
| Property | Type | Notes |
|----------|------|-------|
| `id` | int | PK |
| `primaryColor` | string | MaxLength(20), default `#003F4A` |
| `secondaryColor` | string | MaxLength(20), default `#C9A96B` |
| `borderColor` | string | MaxLength(20) |
| `borderWidth` | float | default `10` |
| `titleEn` | string | MaxLength(200) |
| `titleAr` | string | MaxLength(200) |
| `headerTextEn` | string | MaxLength(500) |
| `headerTextAr` | string | MaxLength(500) |
| `signatoryName` | string? | MaxLength(200) |
| `signatoryTitleEn` | string? | MaxLength(200) |
| `signatoryTitleAr` | string? | MaxLength(200) |
| `signatureImageUrl` | string? | MaxLength(500) |
| `showLogo` | bool | default `true` |
| `logoHeight` | float | default `60` |
| `backgroundImageUrl` | string? | MaxLength(500) |

### Setting
| Property | Type | Notes |
|----------|------|-------|
| `id` | int | PK |
| `siteTitleEn` | string | |
| `siteTitleAr` | string | |
| `logoUrl` | string | Relative path in wwwroot |
| `socialLinksJson` | string | JSON |
| `contactInfo` | string | JSON |

### MediaAccreditation
| Property | Type | Notes |
|----------|------|-------|
| `id` | int | PK |
| `userId` | int | FK → User |
| `category` | AccreditationCategory | Required |
| `status` | ApplicationStatus | default `Pending` |
| `documentUrl` | string | Required, MaxLength(500) |
| `createdAt` | DateTime | |
| `checkedAt` | DateTime? | |
| `checkedByUserId` | int? | FK → User |
| `mediaCard` | MediaCard? | Navigation |

### MediaCard
| Property | Type | Notes |
|----------|------|-------|
| `id` | int | PK |
| `accreditationId` | int | FK → MediaAccreditation |
| `cardNumber` | string | Required, MaxLength(50) |
| `qrCodeData` | string | Required, MaxLength(500) |
| `status` | CardStatus | default `Active` |
| `issuedAt` | DateTime | |
| `expiresAt` | DateTime | |

### AuditLog
| Property | Type | Notes |
|----------|------|-------|
| `id` | int | PK |
| `userId` | int? | |
| `userEmail` | string? | MaxLength(150) |
| `action` | string | Required, MaxLength(100) |
| `tableName` | string | Required, MaxLength(100) |
| `recordId` | string? | |
| `oldValues` | string? | JSON |
| `newValues` | string? | JSON |
| `timestamp` | DateTime | |
| `ipAddress` | string? | MaxLength(50) |

---

## Enums Reference

All enums are in `AppDAL.Entities` and serialized as **string names** in JSON.

### ApplicationStatus
Used by: Volunteer, MediaAccreditation, accreditation/volunteer review endpoints.

| Value | Description |
|-------|-------------|
| `Pending` | Awaiting review |
| `Approved` | Accepted |
| `Rejected` | Declined |
| `Refunded` | Payment refunded |

### AccreditationCategory
| Value |
|-------|
| `Press` |
| `Media` |
| `Staff` |
| `Organizer` |
| `Speaker` |
| `Guest` |
| `VIP` |
| `Trainee` |
| `Volunteer` |
| `BoardMember` |
| `Executive` |
| `Honorary` |
| `Partner` |

### CardStatus
| Value | Description |
|-------|-------------|
| `Active` | Valid card |
| `Expired` | Past expiry date |
| `Suspended` | Temporarily disabled |
| `Revoked` | Permanently cancelled |

### CertificateType
| Value | Description |
|-------|-------------|
| `Training` | Course completion certificate |
| `Volunteer` | Volunteer service certificate |
| `Participation` | Event participation |
| `Custom` | Custom certificate |

### EnrollmentStatus
| Value |
|-------|
| `Pending` |
| `Approved` |
| `Rejected` |

### NewsType
| Value |
|-------|
| `News` |
| `PressRelease` |
| `Announcement` |
| `Statement` |
| `EventAndForum` |
| `Initiative` |

### PartnerCategory
| Value |
|-------|
| `Strategic` |
| `Supporting` |
| `Community` |
| `Media` |
| `EducationalAndCultural` |

### PaymentStatus
| Value |
|-------|
| `PendingVerification` |
| `Paid` |
| `Rejected` |
| `Refunded` |

### PaymentType
| Value | Description |
|-------|-------------|
| `Accreditation` | Media accreditation fee |
| `Course` | Training course fee |
| `Certificate` | Certificate fee |

### VolunteeringArea
| Value |
|-------|
| `MediaAndJournalism` |
| `PhotographyAndProduction` |
| `PublicRelations` |
| `EventManagement` |
| `TranslationAndEditing` |
| `DesignAndCreativeServices` |
| `DigitalMedia` |
| `TrainingPrograms` |
| `AdministrativeSupport` |

---

## Endpoint Summary

| Controller | Total Endpoints | Public |
|------------|-----------------|--------|
| Auth | 8 | 2 |
| Pages | 3 | 2 |
| News | 6 | 3 |
| Partners | 5 | 2 |
| Volunteers | 5 | 0 |
| Training | 9 | 2 |
| ServiceFees | 3 | 2 |
| Payments | 6 | 0 |
| Certificates | 6 | 3 |
| Accreditation | 6 | 1 |
| Settings | 8 | 1 |
| Files | 1 | 0 |
| AuditLogs | 1 | 0 |
| Reports | 3 | 0 |
| **Total** | **71** | **18** |

---

*Generated for GACAM Backend API — TProjectGacam*
