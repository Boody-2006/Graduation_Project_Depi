/* ====================================================================
   MediCare – app.js   (the only JavaScript file of the website)

   TABLE OF CONTENTS
    1. Settings
    2. Small helpers
    3. Translation (English / Arabic)
    4. State (what the app remembers)
    5. API (every call to your backend)
    6. Pages, navigation and visibility
    7. Drawing the pages
    8. Forms and validation
    9. Buttons and pop-ups (actions)
   10. Start

   THERE IS NO BACKEND CODE IN THIS FILE.
   Everything the page needs from a server goes through the Api object
   (section 5). js/mock-server.js is an optional demo helper that fakes
   those calls inside the browser. Nothing in this file depends on it.

   WHAT THE PAGE EXPECTS FROM YOUR API (JSON, camelCase).
   These names are only a suggestion: change them in section 5 if your
   API is different. A name may be a text or an [english, arabic] pair.
   Every list is already filtered by the caller's token (a patient only
   receives his own appointments, a doctor his own patients ...).

     ACCOUNTS
       POST /api/auth/login      {email,password,role} -> {token, user:{id,name,email,role}}
                                 errors: {code:"bad"|"pending"|"rejected"}
       POST /api/auth/register   patient: {role,name,email,phone,password,dateOfBirth,gender}
                                 doctor : {role,name,email,phone,password,specialtyId,city,address,consultationFee}
                                 doctor -> {pending:true}      error: {code:"exists"}
       GET  /api/profile         -> {name,email,phone,photo,
                                     doctor: city,address,consultationFee
                                     patient: dateOfBirth,gender,bloodType,allergies,medicalHistory}
       PUT  /api/profile         same fields (photo = base64 data-URL or "")
       GET  /api/notification-preferences  -> {inAppEnabled,emailEnabled,smsEnabled,reminderHoursBefore}
       PUT  /api/notification-preferences  same fields

     CATALOGUE
       GET  /api/specializations -> [{id,name,nameAr,description,imageUrl}]
       POST /api/specializations / PUT /api/specializations/{id} / DELETE ...   (assistant)
       GET  /api/doctors         -> [{id,name,specialtyId,consultationFee,city,address,availableToday,
                                      approvalStatus:"Pending"|"Approved"|"Rejected",
                                      rejectionReason,averageRating,photo}]
       PUT  /api/doctors/{id}/decision  {status:"Approved"|"Rejected", reason}     (assistant)
       GET  /api/doctors/{id}/slots?date=YYYY-MM-DD -> [{time:"09:00", available:true}]
                                 (the server applies working hours, vacations and existing bookings)

     APPOINTMENTS
       GET  /api/appointments    -> [{id,patientId,doctorId,date,time,
                                      type:"Consultation"|"FollowUp",
                                      status:pending|confirmed|done|cancelled|rejected|rescheduled,
                                      notes,cancelReason,cancelledBy,fee,paymentStatus:"Unpaid"|"Paid",
                                      rescheduledFromId,hasRecord,reviewed}]
       POST /api/appointments    {doctorId,date,time,type,notes}      error: {code:"taken"}
       PUT  /api/appointments/{id}/status      {status,reason}
       PUT  /api/appointments/{id}/reschedule  {date,time}   (old -> "rescheduled", creates the new one)
       PUT  /api/appointments/{id}/payment     {status:"Paid"}                       (assistant)
       POST /api/appointments/{id}/review      {rating 1-5, comment}

     DOCTOR TOOLS
       GET/POST /api/schedule/hours  [{id,dayOfWeek 0-6,startTime,endTime,slotMinutes,isActive}]
       PUT/DELETE /api/schedule/hours/{id}     (PUT {isActive})
       GET/POST /api/schedule/vacations [{id,startDate,endDate,reason}]   DELETE .../{id}
       GET  /api/patients        -> [{id,name,dateOfBirth,age,gender,bloodType,allergies,medicalHistory,isActive}]

     MEDICAL DATA
       GET  /api/records         -> [{id,appointmentId,patientId,doctorId,date,symptoms,diagnosis,treatmentNotes}]
       POST /api/records         {appointmentId,symptoms,diagnosis,treatmentNotes}   (marks the visit "done")
       GET  /api/prescriptions   -> [{id,recordId,patientId,doctorId,date,notes,
                                      items:[{medication,dosage,frequency,durationDays,instructions}]}]
       POST /api/prescriptions   {recordId,notes,items}                              -> {id}
       GET  /api/documents       -> [{id,patientId,recordId,type:"TestResult"|"XRay"|"Other",fileName,url,
                                      uploadedAt,uploadedByName,uploadedByRole}]
       POST /api/documents       multipart: file, patientId, type, recordId?

     BRAIN TUMOR (MRI)
       POST /api/brain-tumor/scans  multipart: file, patientId, recordId?  -> scan (with its first analysis)
       GET  /api/brain-tumor/scans  -> [{id,patientId,doctorId,recordId,fileName,uploadedAt,
                                         analyses:[{id,label:glioma|meningioma|pituitary|notumor,confidence 0..1,
                                                    scores:{glioma,meningioma,pituitary,notumor},modelVersion,
                                                    analyzedAt,reviewStatus:"Pending"|"Confirmed"|"Rejected",
                                                    doctorNotes,reviewedAt}]}]
       PUT  /api/brain-tumor/analyses/{id}/review  {status:"Confirmed"|"Rejected", doctorNotes}

     ASSISTANT
       GET  /api/stats           -> {patients,revenue,monthly:[{label,value}],bySpecialty:[n,n,n,n],
                                     topDoctors:[{doctorId,count}]}
       GET/PUT /api/clinic       {clinicName,phone,street,city,openingTime,closingTime,defaultSlotMinutes}
       PUT  /api/patients/{id}/access   {isActive}

     NOTIFICATIONS
       GET  /api/notifications   -> [{id,type,channel,title,message,isRead,createdAt,appointmentId}]
       PUT  /api/notifications/{id}/read
       SignalR hub  /hubs/notifications   event "Notify" -> {title,message}
   ==================================================================== */

"use strict";


/* ====================================================================
   1. SETTINGS
   ==================================================================== */
const CONFIG = {
  API_BASE: "",              // e.g. "https://localhost:5001"  ("" = same site as this page)
  SESSION_KEY: "medicare_session",
  DEMO_MODE: false,          // switched on by js/mock-server.js only (shows a hint on the sign-in page)
};

const CHART_COLORS = ["var(--primary)", "#7fb3d1", "var(--accent)", "var(--ok)"];


/* ====================================================================
   2. SMALL HELPERS
   ==================================================================== */
const $  = (selector, root = document) => root.querySelector(selector);
const $$ = (selector, root = document) => [...root.querySelectorAll(selector)];

const storage = {
  get(key)        { try { return localStorage.getItem(key); } catch { return null; } },
  set(key, value) { try { localStorage.setItem(key, value); } catch { /* private mode */ } },
  remove(key)     { try { localStorage.removeItem(key); } catch { /* private mode */ } },
};

const todayISO = () => new Date().toISOString().slice(0, 10);
const addDays  = (iso, days) => new Date(Date.parse(iso) + days * 86400000).toISOString().slice(0, 10);
const same     = (a, b) => String(a) === String(b);
const byId     = (list, id) => list.find(item => same(item.id, id));

/* Age is calculated from the date of birth (the ERD stores only the date of birth) */
function ageOf(person) {
  if (!person) return "";
  if (person.age != null) return person.age;
  if (!person.dateOfBirth) return "";
  const born = new Date(person.dateOfBirth), now = new Date();
  let age = now.getFullYear() - born.getFullYear();
  if (now < new Date(now.getFullYear(), born.getMonth(), born.getDate())) age -= 1;
  return age;
}

function formatDateTime(iso) {
  const date = new Date(iso);
  if (Number.isNaN(date.getTime())) return iso || "";
  return date.toLocaleString(State.lang === "ar" ? "ar-EG" : "en-GB", { dateStyle: "medium", timeStyle: "short" });
}

/* Copy a <template> from index.html (already translated) */
function cloneTemplate(id) {
  const node = $(`#${id}`).content.firstElementChild.cloneNode(true);
  translateTree(node);
  return node;
}

/* Fill the data-f="name" parts of a cloned template.
   "name"       -> text
   "name@attr"  -> an attribute (for example "photo@src")
   "name@show"  -> show (true) or hide (false)                         */
function fill(node, values) {
  for (const [key, value] of Object.entries(values)) {
    const [field, attribute] = key.split("@");
    const target = node.matches(`[data-f="${field}"]`) ? node : $(`[data-f="${field}"]`, node);
    if (!target) continue;
    if (attribute === "show")  target.hidden = !value;
    else if (attribute)        target.setAttribute(attribute, value);
    else                       target.textContent = value;
  }
}

/* A round picture: the first letter of the name, replaced by the photo when it loads */
function setAvatar(element, name, photoUrl) {
  element.textContent = (name || "?").trim().charAt(0).toUpperCase();
  if (!photoUrl) return;
  const image = new Image();
  image.alt = "";
  image.onload = () => element.replaceChildren(image);
  image.src = photoUrl;
}

/* Photos in a .slot that cannot load show their file name instead */
document.addEventListener("error", event => {
  const image = event.target;
  if (image instanceof HTMLImageElement && image.closest(".slot")) {
    image.closest(".slot").classList.add("missing");
  }
}, true);

/* Photos that already failed before this script started (the ones written in index.html) */
function markMissingPhotos() {
  $$(".slot img").forEach(image => {
    if (image.complete && image.naturalWidth === 0) image.closest(".slot").classList.add("missing");
  });
}

function toast(message) {
  const element = document.createElement("div");
  element.className = "toast";
  element.textContent = message;
  $("#toasts").append(element);
  setTimeout(() => element.remove(), 4500);
}

function openModal(id) {
  const dialog = $(`#${id}`);
  if (!dialog.open) dialog.showModal();
}

function closeModal(id) {
  const dialog = $(`#${id}`);
  if (dialog.open) dialog.close();
}

/* Clicking the dark area around a pop-up closes it */
document.addEventListener("click", event => {
  if (event.target instanceof HTMLDialogElement) event.target.close();
});


/* ====================================================================
   3. TRANSLATION
   English is written inside index.html. Arabic is stored here.

   AR  : Arabic for texts that already exist in index.html  (data-i18n="key")
   TXT : texts written by JavaScript, as [english, arabic]
         (a TXT key can also be used inside index.html)
   ==================================================================== */
const AR = {
  "nav.home": "الرئيسية", "nav.specialties": "التخصصات", "nav.doctors": "الأطباء", "nav.about": "من نحن",
  "nav.findDoctors": "ابحث عن دكتور", "nav.appointments": "المواعيد", "nav.records": "السجل الطبي",
  "nav.dashboard": "لوحة التحكم", "nav.schedule": "الجدول", "nav.patients": "المرضى",
  "nav.mri": "تحليل الرنين", "nav.manageDoctors": "الأطباء", "nav.clinic": "العيادة",
  "nav.signIn": "تسجيل الدخول", "nav.register": "سجّل الآن",

  "menu.home": "الرئيسية", "menu.profile": "ملفي الشخصي", "menu.website": "الموقع", "menu.signOut": "خروج",

  "home.title": "صحتك، بمواعيد منظمة وعناية حقيقية",
  "home.lead": "اختار التخصص المناسب، واحجز في ثواني، واحتفظ بكل سجلاتك وروشتاتك وأشعتك في مكان واحد آمن.",
  "home.cta": "احجز موعد",
  "home.statDoctors": "طبيب", "home.statSpecs": "تخصص", "home.statBooking": "حجز أونلاين",
  "home.specTitle": "تصفح حسب التخصص", "home.specText": "اختار التخصص المطلوب وشوف الأطباء المتاحين.",
  "home.howTitle": "إزاي بتشتغل",
  "home.how1": "ابحث", "home.how1Text": "فلتر الأطباء بالتخصص والمدينة والتوفر.",
  "home.how2": "احجز", "home.how2Text": "اختار ميعاد متاح، والحجز المزدوج مش بيحصل.",
  "home.how3": "زور وتابع", "home.how3Text": "استلم الروشتة والسجل الطبي بعد كل زيارة.",
  "home.featuredTitle": "أطباء تقدر تثق فيهم", "home.allDoctors": "كل الأطباء",
  "home.aiTitle": "فحص مبدئي لأشعة المخ بالذكاء الاصطناعي",
  "home.aiText": "الدكتور يرفع صورة الرنين ويستلم تصنيف مع نسبة ثقة تساعده في المراجعة.",
  "home.ai1": "ورم دبقي أو سحائي أو نخامي أو لا يوجد ورم",
  "home.ai2": "نسبة ثقة مع كل نتيجة",
  "home.ai3": "مساعد فقط ولا يغني عن طبيب الأشعة",
  "home.faqTitle": "أسئلة شائعة",
  "home.q1": "أحجز موعد إزاي؟", "home.a1": "سجّل حساب مريض، افتح صفحة الأطباء، اختار ميعاد متاح وأكّد.",
  "home.q2": "أقدر ألغي أو أغيّر الميعاد؟", "home.a2": "أيوه، من صفحة المواعيد قبل الزيارة.",
  "home.q3": "الروشتات فين؟", "home.a3": "في صفحة السجل الطبي، وتقدر تطبعها في أي وقت.",
  "home.q4": "الدكتور بينضم إزاي؟", "home.a4": "الدكتور بيسجّل والمساعد بيعتمد الحساب.",
  "home.ctaTitle": "جاهز تحجز زيارتك؟",

  "doctors.title": "الأطباء", "doctors.search": "ابحث بالاسم أو المدينة",
  "doctors.availableToday": "متاح النهارده", "doctors.empty": "مفيش أطباء مطابقين للبحث.", "doctors.fee": "الكشف",

  "about.title": "عن ميديكير",
  "about.text": "ميديكير بتربط المرضى والأطباء وفريق العيادة في منصة واحدة للمواعيد والسجلات والروشتات وفحص أشعة المخ.",
  "about.v1": "المريض أولاً", "about.v1Text": "حجز بسيط وسجلات واضحة.",
  "about.v2": "وقت الطبيب", "about.v2Text": "الجداول والطلبات والروشتات في مكان واحد.",
  "about.v3": "الخصوصية", "about.v3Text": "صلاحيات حسب الدور لكل سجل.",

  "auth.signInTitle": "تسجيل الدخول", "auth.signInButton": "دخول", "auth.noAccount": "إنشاء حساب جديد",
  "auth.demoHint": "وضع تجريبي: أي إيميل وباسورد يعرضلك الدور المختار.",
  "auth.registerTitle": "إنشاء حساب جديد", "auth.registerButton": "إنشاء الحساب", "auth.haveAccount": "عندي حساب بالفعل",

  "field.email": "البريد الإلكتروني", "field.password": "كلمة المرور", "field.name": "الاسم بالكامل",
  "field.phone": "الموبايل", "field.specialty": "التخصص", "field.city": "المدينة", "field.address": "العنوان",
  "field.fee": "سعر الكشف (ج.م)", "field.confirmPassword": "تأكيد كلمة المرور",
  "field.dateOfBirth": "تاريخ الميلاد", "field.gender": "النوع", "field.bloodType": "فصيلة الدم",
  "field.allergies": "الحساسية (افصل بينها بفاصلة)", "field.history": "التاريخ المرضي",
  "field.clinic": "اسم العيادة", "field.street": "الشارع", "field.openingTime": "ميعاد الفتح", "field.closingTime": "ميعاد الإغلاق",
  "field.defaultSlot": "مدة الموعد الافتراضية بالدقايق",
  "field.day": "اليوم", "field.startTime": "من", "field.endTime": "إلى", "field.slotMinutes": "دقايق لكل موعد",
  "field.startDate": "من", "field.endDate": "إلى", "field.reason": "السبب",
  "field.notes": "ملاحظات للدكتور (اختياري)", "field.documentType": "نوع المستند", "field.file": "الملف",
  "field.visitOptional": "الزيارة (اختياري)", "field.doctorNotes": "ملاحظات الدكتور",
  "field.nameEn": "الاسم بالإنجليزي", "field.nameAr": "الاسم بالعربي", "field.description": "الوصف",
  "field.imageUrl": "رابط الصورة (اختياري)",
  "gender.male": "ذكر", "gender.female": "أنثى",

  "app.greeting": "يوم سعيد",
  "patient.next": "الموعد القادم", "patient.noAppointment": "مفيش مواعيد محجوزة.",
  "records.prescriptions": "الروشتات", "records.visits": "الزيارات", "records.documents": "المستندات", "records.scans": "أشعة الرنين",
  "table.date": "التاريخ", "table.time": "الوقت", "table.doctor": "الدكتور", "table.patient": "المريض",
  "table.status": "الحالة", "table.type": "النوع", "table.fee": "الكشف", "table.payment": "الدفع",

  "doctor.statToday": "النهارده", "doctor.statRequests": "طلبات للمراجعة", "doctor.statConfirmed": "مؤكد",
  "doctor.statRating": "متوسط التقييم", "doctor.requests": "طلبات للمراجعة", "doctor.visits": "زيارات محتاجة سجل",
  "schedule.hours": "ساعات الشغل", "schedule.vacations": "الإجازات",

  "mri.upload": "ارفع صورة الرنين", "mri.analyze": "حلل", "mri.result": "النتيجة", "mri.noResult": "مفيش نتيجة لسه.",
  "mri.confidence": "الثقة", "mri.scores": "احتمال كل تصنيف", "mri.model": "الموديل", "mri.history": "الأشعة السابقة",
  "mri.disclaimer": "مساعد فقط ولا يغني عن رأي طبيب الأشعة.", "mri.verifyTitle": "راجع نتيجة الذكاء الاصطناعي",

  "assistant.revenue": "الإيرادات (ج.م)", "assistant.monthly": "المواعيد كل شهر",
  "assistant.bySpecialty": "حسب التخصص", "assistant.topDoctors": "أكتر الدكاترة حجزاً",
  "specialties.add": "أضف تخصص", "specialties.title": "تخصص",

  "profile.title": "ملفي الشخصي", "profile.changePhoto": "غيّر الصورة", "profile.removePhoto": "احذف",
  "prefs.title": "تفضيلات الإشعارات", "prefs.inApp": "إشعارات داخل الموقع", "prefs.email": "إيميل", "prefs.sms": "رسالة SMS",
  "prefs.reminder": "ذكّرني قبل الموعد بكام ساعة",

  "common.save": "حفظ", "common.close": "إغلاق", "common.cancel": "إلغاء", "common.print": "طباعة",
  "common.book": "احجز", "common.add": "إضافة", "common.confirm": "تأكيد",
  "booking.hint": "الأوقات الرمادي مش متاحة.",
  "documents.upload": "ارفع مستند",
  "record.title": "سجل الزيارة", "record.symptoms": "الأعراض", "record.diagnosis": "التشخيص", "record.treatment": "خطة العلاج",
  "rx.title": "روشتة", "rx.addMedication": "إضافة دواء", "rx.notes": "تعليمات عامة (اختياري)",
  "rx.medication": "الدواء", "rx.dosage": "الجرعة (مثلاً 500 مجم)", "rx.frequency": "كام مرة (مثلاً 3 مرات يومياً)",
  "rx.duration": "عدد الأيام", "rx.instructions": "ملاحظات",
  "rating.title": "قيّم زيارتك", "rating.comment": "تعليق (اختياري)",
  "notifications.title": "الإشعارات",
  "footer.tagline": "رعاية بتحترم مواعيدك", "footer.rights": "جميع الحقوق محفوظة.",
};

const TXT = {
  "role.patient":   ["Patient", "مريض"],
  "role.doctor":    ["Doctor", "دكتور"],
  "role.assistant": ["Assistant", "مساعد"],

  "status.pending":     ["Pending", "منتظر"],
  "status.confirmed":   ["Confirmed", "مؤكد"],
  "status.done":        ["Done", "تم"],
  "status.cancelled":   ["Cancelled", "ملغي"],
  "status.rejected":    ["Rejected", "مرفوض"],
  "status.rescheduled": ["Rescheduled", "تم تغيير الميعاد"],
  "status.active":      ["Active", "مفعّل"],
  "status.blocked":     ["Blocked", "موقوف"],
  "status.on":          ["On", "شغال"],
  "status.off":         ["Off", "موقوف"],

  "approval.Pending":  ["Pending approval", "منتظر الاعتماد"],
  "approval.Approved": ["Approved", "معتمد"],
  "approval.Rejected": ["Rejected", "مرفوض"],

  "review.Pending":   ["Not reviewed", "لم تتم المراجعة"],
  "review.Confirmed": ["Confirmed", "مؤكد"],
  "review.Rejected":  ["Rejected", "مرفوض"],

  "type.Consultation": ["Consultation", "استشارة"],
  "type.FollowUp":     ["Follow-up", "متابعة"],
  "payment.Unpaid":    ["Unpaid", "غير مدفوع"],
  "payment.Paid":      ["Paid", "مدفوع"],
  "docType.TestResult": ["Test result", "نتيجة تحليل"],
  "docType.XRay":       ["X-ray", "أشعة"],
  "docType.Other":      ["Other", "أخرى"],

  "day.0": ["Sunday", "الأحد"],   "day.1": ["Monday", "الاثنين"],  "day.2": ["Tuesday", "الثلاثاء"],
  "day.3": ["Wednesday", "الأربعاء"], "day.4": ["Thursday", "الخميس"], "day.5": ["Friday", "الجمعة"],
  "day.6": ["Saturday", "السبت"],

  "btn.reschedule":   ["Reschedule", "غيّر الميعاد"],
  "btn.cancel":       ["Cancel", "إلغاء"],
  "btn.rate":         ["Rate", "قيّم"],
  "btn.confirm":      ["Confirm", "تأكيد"],
  "btn.reject":       ["Reject", "رفض"],
  "btn.approve":      ["Approve", "اعتماد"],
  "btn.record":       ["Visit record", "سجل الزيارة"],
  "btn.prescription": ["Write prescription", "اكتب روشتة"],
  "btn.view":         ["View", "عرض"],
  "btn.print":        ["Print", "طباعة"],
  "btn.markPaid":     ["Mark as paid", "تم الدفع"],
  "btn.patientFile":  ["Patient file", "ملف المريض"],
  "btn.verify":       ["Verify", "راجع"],
  "btn.edit":         ["Edit", "تعديل"],
  "btn.delete":       ["Delete", "حذف"],
  "btn.block":        ["Block access", "إيقاف الدخول"],
  "btn.unblock":      ["Allow access", "السماح بالدخول"],
  "btn.turnOff":      ["Turn off", "إيقاف"],
  "btn.turnOn":       ["Turn on", "تشغيل"],

  "unit.doctors": ["doctors", "أطباء"],
  "unit.egp":     ["EGP", "ج.م"],
  "unit.years":   ["yrs", "سنة"],
  "unit.days":    ["days", "يوم"],
  "unit.min":     ["min", "دقيقة"],

  "label.patient":    ["Patient", "المريض"],
  "label.doctor":     ["Doctor", "الدكتور"],
  "label.reason":     ["Reason", "السبب"],
  "label.gender":     ["Gender", "النوع"],
  "label.bloodType":  ["Blood type", "فصيلة الدم"],
  "label.allergies":  ["Allergies", "الحساسية"],
  "label.history":    ["History", "التاريخ المرضي"],
  "label.uploadedBy": ["Uploaded by", "رفعه"],
  "label.notes":      ["Notes", "ملاحظات"],
  "label.appointments": ["appointments", "موعد"],
  "label.from":       ["from", "من"],
  "label.to":         ["to", "إلى"],
  "gender.Male":      ["Male", "ذكر"],
  "gender.Female":    ["Female", "أنثى"],
  "rx.sheetTitle":    ["Prescription", "روشتة"],
  "reason.cancel":            ["Cancel the appointment", "إلغاء الموعد"],
  "reason.rejectAppointment": ["Reject the request", "رفض الطلب"],
  "reason.rejectDoctor":      ["Reject the doctor", "رفض الدكتور"],

  "mri.glioma":     ["Glioma", "ورم دبقي"],
  "mri.meningioma": ["Meningioma", "ورم سحائي"],
  "mri.pituitary":  ["Pituitary tumor", "ورم الغدة النخامية"],
  "mri.notumor":    ["No tumor detected", "لا يوجد ورم"],

  "filter.all":    ["All specialties", "كل التخصصات"],
  "filter.choose": ["Choose…", "اختر…"],
  "filter.none":   ["None", "بدون"],

  "msg.nothing":        ["Nothing here yet", "لا يوجد شيء بعد"],
  "msg.saved":          ["Saved", "تم الحفظ"],
  "msg.booked":         ["Done. The appointment is waiting for the doctor", "تم. الموعد منتظر تأكيد الدكتور"],
  "msg.loginFirst":     ["Please sign in as a patient to book", "سجّل دخول كمريض عشان تحجز"],
  "msg.pendingDoctor":  ["Account created. Doctor accounts are activated after approval.", "تم إنشاء الحساب. حسابات الدكاترة بتتفعّل بعد الاعتماد."],
  "msg.chooseRating":   ["Please choose a rating", "اختار تقييم"],
  "msg.thanks":         ["Thank you for your review", "شكراً على تقييمك"],
  "msg.noSlots":        ["No available times on this day", "مفيش أوقات متاحة في اليوم ده"],
  "msg.confirmDelete":  ["Delete this item?", "تحذف العنصر ده؟"],
  "msg.needRecord":     ["Write the visit record first", "اكتب سجل الزيارة الأول"],
  "msg.noPatients":     ["No patients yet", "مفيش مرضى لسه"],
  "msg.analysed":       ["The scan was analysed", "تم تحليل الصورة"],

  "err.required": ["is required", "مطلوب"],
  "err.email":    ["Enter a valid email address", "اكتب إيميل صحيح"],
  "err.phone":    ["Enter a valid phone number (10–15 digits)", "اكتب رقم موبايل صحيح (10–15 رقم)"],
  "err.minLength":["Must be at least {n} characters", "لازم يكون {n} حروف على الأقل"],
  "err.range":    ["is out of range", "خارج الحد المسموح"],
  "err.after":    ["must be later than the start", "لازم يكون بعد البداية"],
  "err.match":    ["Passwords do not match", "كلمتا المرور غير متطابقتين"],
  "err.badLogin": ["Wrong email, password or role", "الإيميل أو كلمة المرور أو الدور غلط"],
  "err.pending":  ["Your doctor account is still waiting for approval", "حساب الدكتور لسه منتظر الاعتماد"],
  "err.rejected": ["Your doctor account was rejected", "حساب الدكتور اتّرفض"],
  "err.exists":   ["This email is already registered", "الإيميل ده مسجّل قبل كده"],
  "err.blocked":  ["Your account is blocked. Please contact the clinic.", "حسابك موقوف. تواصل مع العيادة."],
  "err.taken":    ["This time was just booked, choose another one", "الميعاد ده اتحجز، اختار ميعاد تاني"],
  "err.generic":  ["Something went wrong, please try again", "حصلت مشكلة، حاول تاني"],
};

/* English text of a dynamic message: t("status.pending") */
function t(key) {
  const entry = TXT[key];
  return entry ? entry[State.lang === "ar" ? 1 : 0] : key;
}

/* A name or text that is either "text" or ["english", "arabic"] */
function tr(value) {
  return Array.isArray(value) ? value[State.lang === "ar" ? 1 : 0] : (value ?? "");
}

function arabicFor(key) {
  return AR[key] ?? (TXT[key] ? TXT[key][1] : undefined);
}

/* Switch the text of one element (and its children) to the current language */
function translateTree(root) {
  const elements = [root, ...$$("[data-i18n], [data-i18n-placeholder]", root)];
  for (const element of elements) {
    if (element.dataset.i18n) {
      if (element.dataset.en === undefined) element.dataset.en = element.textContent;
      element.textContent = State.lang === "ar" ? (arabicFor(element.dataset.i18n) ?? element.dataset.en) : element.dataset.en;
    }
    if (element.dataset.i18nPlaceholder) {
      if (element.dataset.enPlaceholder === undefined) element.dataset.enPlaceholder = element.placeholder;
      element.placeholder = State.lang === "ar" ? (arabicFor(element.dataset.i18nPlaceholder) ?? element.dataset.enPlaceholder) : element.dataset.enPlaceholder;
    }
  }
}

function applyLanguageAndTheme() {
  const html = document.documentElement;
  html.lang = State.lang;
  html.dir = State.lang === "ar" ? "rtl" : "ltr";
  html.dataset.theme = State.theme;
  translateTree(document.body);
}


/* ====================================================================
   4. STATE
   ==================================================================== */
const State = {
  lang: "en",
  theme: "light",

  session: null,               // {token, user:{id,name,email,role}}

  // data from the backend
  specialties: [],
  doctors: [],
  patients: [],
  appointments: [],
  records: [],
  prescriptions: [],
  documents: [],
  scans: [],
  notifications: [],
  stats: {},
  schedule: { hours: [], vacations: [] },
  clinic: null,
  preferences: null,
  profile: null,

  // what the user is doing
  filters: { query: "", specialty: "", availableToday: false },
  authRole: "patient",
  loginNotice: "",
  booking: null,               // {doctorId, rescheduleId}
  rating: null,                // {appointmentId, value}
  reason: null,                // {kind, id}
  recordAppointmentId: null,
  prescriptionRecordId: null,
  documentPatientId: null,
  patientFileId: null,
  analysisId: null,
  editSpecialtyId: null,
  lastScanId: null,
  profileDraft: null,          // {photo}
  profileDirty: false,
  realtimeTimer: null,
  hub: null,
};

const role   = () => State.session?.user.role;
const userId = () => State.session?.user.id;


/* ====================================================================
   5. API  (one function per backend call)
   ==================================================================== */
async function request(method, path, body) {
  const headers = {};
  let payload;
  if (State.session?.token) headers.Authorization = `Bearer ${State.session.token}`;
  if (body instanceof FormData) {
    payload = body;
  } else if (body !== undefined) {
    headers["Content-Type"] = "application/json";
    payload = JSON.stringify(body);
  }

  const response = await fetch(CONFIG.API_BASE + path, { method, headers, body: payload });
  if (response.status === 401 && State.session) { signOut(); throw new Error("unauthorized"); }

  const data = await response.json().catch(() => null);
  if (!response.ok) throw new Error(data?.code || String(response.status));
  return data;
}

const get   = path => request("GET", path);
const quiet = (path, fallback = []) => get(path).catch(() => fallback);

const Api = {
  /* accounts */
  login:    (email, password, userRole) => request("POST", "/api/auth/login", { email, password, role: userRole }),
  register: data => request("POST", "/api/auth/register", data),
  getProfile:  () => get("/api/profile"),
  saveProfile: data => request("PUT", "/api/profile", data),
  savePreferences: data => request("PUT", "/api/notification-preferences", data),

  /* what a visitor can see */
  publicData: async () => {
    const [specialties, doctors] = await Promise.all([get("/api/specializations"), get("/api/doctors")]);
    return { specialties, doctors };
  },

  /* everything a logged-in user needs (the server filters it by role) */
  bootstrap: async () => {
    const [specialties, doctors, patients, appointments, records, prescriptions, documents, scans, notifications, preferences] =
      await Promise.all([
        get("/api/specializations"), get("/api/doctors"),
        quiet("/api/patients"), quiet("/api/appointments"), quiet("/api/records"),
        quiet("/api/prescriptions"), quiet("/api/documents"), quiet("/api/brain-tumor/scans"),
        quiet("/api/notifications"), quiet("/api/notification-preferences", null)]);

    const data = { specialties, doctors, patients, appointments, records, prescriptions, documents, scans, notifications, preferences };
    if (role() === "doctor") {
      const [hours, vacations] = await Promise.all([quiet("/api/schedule/hours"), quiet("/api/schedule/vacations")]);
      data.schedule = { hours, vacations };
    }
    if (role() === "assistant") {
      data.stats = await quiet("/api/stats", {});
      data.clinic = await quiet("/api/clinic", null);
    }
    return data;
  },

  /* appointments */
  slots:      (doctorId, date) => get(`/api/doctors/${doctorId}/slots?date=${date}`),
  book:       data => request("POST", "/api/appointments", data),
  setStatus:  (id, status, reason) => request("PUT", `/api/appointments/${id}/status`, { status, reason }),
  reschedule: (id, data) => request("PUT", `/api/appointments/${id}/reschedule`, data),
  markPaid:   id => request("PUT", `/api/appointments/${id}/payment`, { status: "Paid" }),
  review:     data => request("POST", `/api/appointments/${data.appointmentId}/review`, data),

  /* medical data */
  createRecord:    data => request("POST", "/api/records", data),
  addPrescription: data => request("POST", "/api/prescriptions", data),
  uploadDocument:  form => request("POST", "/api/documents", form),
  createScan:      form => request("POST", "/api/brain-tumor/scans", form),
  reviewAnalysis:  (id, data) => request("PUT", `/api/brain-tumor/analyses/${id}/review`, data),

  /* doctor: working hours and vacations */
  addHours:       data => request("POST", "/api/schedule/hours", data),
  setHoursActive: (id, isActive) => request("PUT", `/api/schedule/hours/${id}`, { isActive }),
  deleteHours:    id => request("DELETE", `/api/schedule/hours/${id}`),
  addVacation:    data => request("POST", "/api/schedule/vacations", data),
  deleteVacation: id => request("DELETE", `/api/schedule/vacations/${id}`),

  /* assistant */
  decideDoctor:     (id, status, reason) => request("PUT", `/api/doctors/${id}/decision`, { status, reason }),
  setPatientAccess: (id, isActive) => request("PUT", `/api/patients/${id}/access`, { isActive }),
  saveClinic:       data => request("PUT", "/api/clinic", data),
  saveSpecialty:    (data, id) => id == null ? request("POST", "/api/specializations", data)
                                             : request("PUT", `/api/specializations/${id}`, data),
  deleteSpecialty:  id => request("DELETE", `/api/specializations/${id}`),

  /* notifications */
  markNotificationRead: id => request("PUT", `/api/notifications/${id}/read`),
};

/* Load data into State */
async function loadPublicData() {
  const data = await Api.publicData();
  State.specialties = data.specialties;
  State.doctors = data.doctors;
}

async function refreshData() {
  if (!State.session) return loadPublicData();
  Object.assign(State, await Api.bootstrap());
  State.profile = await Api.getProfile();
}

/* Real-time notifications */
function startRealtime() {
  stopRealtime();
  const user = State.session?.user;
  if (!user) return;

  if (Api.subscribe) {                       // only exists when js/mock-server.js is loaded
    State.realtimeTimer = Api.subscribe(user, onNotification);
  } else if (window.signalR) {
    State.hub = new signalR.HubConnectionBuilder()
      .withUrl(CONFIG.API_BASE + "/hubs/notifications", { accessTokenFactory: () => State.session.token })
      .withAutomaticReconnect()
      .build();
    State.hub.on("Notify", onNotification);
    State.hub.start().catch(() => {});
  }
}

function stopRealtime() {
  clearTimeout(State.realtimeTimer);
  State.hub?.stop();
  State.hub = null;
}

async function onNotification(payload) {
  const text = typeof payload === "string" || Array.isArray(payload) ? payload : (payload?.title || payload?.message || "");
  toast(tr(text));
  try { await refreshData(); } catch { /* keep the old data */ }
  renderAll();
}

/* Session */
async function startSession(result) {
  State.session = result;
  storage.set(CONFIG.SESSION_KEY, JSON.stringify(result));
  State.profileDirty = false;
  State.profileDraft = null;
  await refreshData();
  startRealtime();
  goTo("app/home");
}

async function signOut() {
  stopRealtime();
  State.session = null;
  Object.assign(State, {
    profile: null, patients: [], appointments: [], records: [], prescriptions: [], documents: [],
    scans: [], notifications: [], preferences: null, clinic: null, stats: {},
    schedule: { hours: [], vacations: [] }, patientFileId: null, lastScanId: null,
  });
  storage.remove(CONFIG.SESSION_KEY);
  await loadPublicData().catch(() => {});
  goTo("home");
}


/* ====================================================================
   6. PAGES, NAVIGATION AND VISIBILITY
   Routes look like  #/home   #/doctors   #/app/appointments
   ==================================================================== */
const currentRoute = () => location.hash.replace(/^#\/?/, "") || "home";

function goTo(route) {
  if (location.hash === `#/${route}`) renderAll();
  else location.hash = `#/${route}`;
}

/* data-auth="in|out" and data-roles="patient doctor" decide who sees an element */
function isAllowed(element) {
  const loggedIn = Boolean(State.session);
  if (element.dataset.auth === "in"  && !loggedIn) return false;
  if (element.dataset.auth === "out" &&  loggedIn) return false;
  if (element.dataset.roles && !element.dataset.roles.split(" ").includes(role())) return false;
  return true;
}

function applyVisibility() {
  $$("[data-auth], [data-roles]").forEach(element => {
    if (element.classList.contains("page")) return;      // pages are handled in showPage()
    const allowed = isAllowed(element);
    element.hidden = !allowed;
    if (element.tagName === "FIELDSET") element.disabled = !allowed;
  });
  $$("[data-demo-only]").forEach(element => { element.hidden = !CONFIG.DEMO_MODE; });
}

/* Show the page that matches the address. Returns false if it had to redirect. */
function showPage() {
  const loggedIn = Boolean(State.session);
  let route = currentRoute();

  if (route.startsWith("app/") && !loggedIn) { goTo("login"); return false; }
  if ((route === "login" || route === "register") && loggedIn) { goTo("app/home"); return false; }

  const pages = $$(".page");
  let visible = pages.filter(page => page.dataset.page === route && isAllowed(page));
  if (!visible.length) {                                  // unknown address -> the home page
    route = loggedIn ? "app/home" : "home";
    visible = pages.filter(page => page.dataset.page === route && isAllowed(page));
  }

  pages.forEach(page => { page.hidden = !visible.includes(page); });
  $$(".main-nav a").forEach(link => link.classList.toggle("active", link.dataset.route === route));
  return true;
}

function renderAll() {
  applyVisibility();
  if (!showPage()) return;

  renderHeader();
  renderPublicPages();
  renderAuthForms();

  if (State.session) {
    ({ patient: renderPatientPages, doctor: renderDoctorPages, assistant: renderAssistantPages })[role()]();
    renderProfile();
    renderPatientFile();
  }
  translateTree(document.body);
}


/* ====================================================================
   7. DRAWING THE PAGES
   ==================================================================== */

/* ---------- lookups ---------- */
const specialtyLabel = specialty => (State.lang === "ar" && specialty?.nameAr) ? specialty.nameAr : tr(specialty?.name);
const doctorName     = id => tr(byId(State.doctors, id)?.name);
const patientName    = id => tr(byId(State.patients, id)?.name);
const specialtyName  = id => specialtyLabel(byId(State.specialties, id));

const isApproved = doctor => doctor.approvalStatus === "Approved";
const isLive     = appointment => !["cancelled", "rejected", "rescheduled"].includes(appointment.status);
const byDateTime = (a, b) => (a.date + a.time).localeCompare(b.date + b.time);

function statusChip(element, className, label) {
  element.className = `chip chip-${className}`;
  element.textContent = label;
}

function actionButton(labelKey, action, id, extraClass = "") {
  const button = document.createElement("button");
  button.type = "button";
  button.className = `btn btn-outline btn-sm ${extraClass}`.trim();
  button.dataset.action = action;
  button.dataset.id = id;
  button.textContent = t(labelKey);
  return button;
}

function renderList(list, nodes) {
  if (nodes.length) { list.replaceChildren(...nodes); return; }
  const empty = document.createElement("li");
  empty.className = "list-row muted";
  empty.textContent = t("msg.nothing");
  list.replaceChildren(empty);
}

function renderRows(tbody, nodes, columnCount) {
  if (nodes.length) { tbody.replaceChildren(...nodes); return; }
  const row = document.createElement("tr");
  const cell = document.createElement("td");
  cell.colSpan = columnCount;
  cell.className = "muted";
  cell.textContent = t("msg.nothing");
  row.append(cell);
  tbody.replaceChildren(row);
}

/* One row of an appointment table.
   columns   = which <td data-col="..."> to keep
   actionsFor(appointment) = the buttons of the last column */
function appointmentRow(appointment, columns, actionsFor = () => []) {
  const node = cloneTemplate("tpl-appointment-row");
  fill(node, {
    date: appointment.date,
    time: appointment.time,
    patient: patientName(appointment.patientId),
    doctor: doctorName(appointment.doctorId),
    type: t(`type.${appointment.type || "Consultation"}`),
    fee: `${appointment.fee ?? 0} ${t("unit.egp")}`,
    by: appointment.cancelledBy ? `(${t("role." + appointment.cancelledBy)})` : "",
    reason: appointment.cancelReason || "",
  });
  statusChip($('[data-f="status"]', node), appointment.status, t(`status.${appointment.status}`));
  const payment = appointment.paymentStatus || "Unpaid";
  statusChip($('[data-f="payment"]', node), payment.toLowerCase(), t(`payment.${payment}`));

  $$("[data-col]", node).forEach(cell => { if (!columns.includes(cell.dataset.col)) cell.remove(); });
  $('[data-f="actions"]', node)?.append(...actionsFor(appointment));
  return node;
}

/* A row with a round picture: doctors, patients, specialties */
function personRow({ name, meta, photo, note, status, statusClass, actions = [] }) {
  const node = cloneTemplate("tpl-person-row");
  fill(node, { name, meta, note: note || "", "note@show": Boolean(note), "status@show": Boolean(status) });
  setAvatar($('[data-f="avatar"]', node), name, photo);
  if (status) statusChip($('[data-f="status"]', node), statusClass, status);
  $('[data-f="actions"]', node).append(...actions);
  return node;
}

/* ---------- rows shared by several pages ---------- */
function visitRow(record, actions = []) {
  const node = cloneTemplate("tpl-visit-row");
  fill(node, {
    date: record.date,
    doctor: doctorName(record.doctorId),
    diagnosis: record.diagnosis,
    symptoms: record.symptoms,
    treatment: record.treatmentNotes,
  });
  $('[data-f="actions"]', node).append(...actions);
  return node;
}

function medicationLine(item) {
  const parts = [`${item.medication} ${item.dosage}`, item.frequency, `${item.durationDays} ${t("unit.days")}`];
  return parts.join(" · ") + (item.instructions ? ` (${item.instructions})` : "");
}

function prescriptionRow(prescription) {
  const node = cloneTemplate("tpl-prescription-row");
  fill(node, {
    date: prescription.date,
    doctor: doctorName(prescription.doctorId),
    items: prescription.items.map(medicationLine).join("\n"),
  });
  $('[data-f="actions"]', node).append(actionButton("btn.view", "view-prescription", prescription.id));
  return node;
}

function documentRow(document) {
  const node = cloneTemplate("tpl-document-row");
  fill(node, {
    type: t(`docType.${document.type}`),
    file: document.fileName,
    "file@href": document.url || "#",
    meta: `${formatDateTime(document.uploadedAt)} · ${t("label.uploadedBy")}: ${tr(document.uploadedByName)} (${t("role." + document.uploadedByRole)})`,
  });
  return node;
}

const latestAnalysis = scan => scan.analyses[scan.analyses.length - 1];

function scanRow(scan, { showPatient = false, canVerify = false } = {}) {
  const analysis = latestAnalysis(scan);
  const node = cloneTemplate("tpl-scan-row");
  const percent = Math.round(analysis.confidence * 100);
  fill(node, {
    label: `${t(`mri.${analysis.label}`)} · ${percent}%`,
    meta: [formatDateTime(scan.uploadedAt), showPatient ? patientName(scan.patientId) : doctorName(scan.doctorId)].join(" · "),
    notes: analysis.doctorNotes ? `${t("label.notes")}: ${analysis.doctorNotes}` : "",
    "notes@show": Boolean(analysis.doctorNotes),
  });
  statusChip($('[data-f="status"]', node), analysis.reviewStatus.toLowerCase(), t(`review.${analysis.reviewStatus}`));
  if (canVerify && analysis.reviewStatus === "Pending") {
    $('[data-f="actions"]', node).append(actionButton("btn.verify", "verify-analysis", analysis.id));
  }
  return node;
}

/* ---------- header ---------- */
function renderHeader() {
  $("#brand-link").href = State.session ? "#/app/home" : "#/home";
  $("#lang-button").textContent = State.lang === "ar" ? "EN" : "ع";

  const user = State.session?.user;
  if (!user) return;

  const name = tr(State.profile?.name || user.name);
  $("#account-name").textContent = name;
  $("#account-email").textContent = user.email || "";
  setAvatar($("#header-avatar"), name, State.profile?.photo);

  const unread = State.notifications.filter(n => !n.isRead).length;
  const badge = $("#notification-badge");
  badge.hidden = unread === 0;
  badge.textContent = unread;

  $$(".user-name").forEach(element => { element.textContent = name; });
  $$(".user-first-name").forEach(element => { element.textContent = name.split(" ")[0]; });
}

/* ---------- public pages: specialties and doctors ---------- */
function specialtyCard(specialty) {
  const node = cloneTemplate("tpl-specialty");
  const count = State.doctors.filter(d => isApproved(d) && same(d.specialtyId, specialty.id)).length;
  fill(node, {
    name: specialtyLabel(specialty),
    count: `${count} ${t("unit.doctors")}`,
    "slot@data-file": specialty.imageUrl ? "" : `spec-${specialty.id}.jpg`,
    "image@src": specialty.imageUrl || `img/spec-${specialty.id}.jpg`,
  });
  node.dataset.id = specialty.id;
  return node;
}

function doctorCard(doctor) {
  const node = cloneTemplate("tpl-doctor-card");
  const canBook = !State.session || role() === "patient";
  fill(node, {
    name: tr(doctor.name),
    specialty: specialtyName(doctor.specialtyId),
    city: tr(doctor.city),
    fee: `${doctor.consultationFee} ${t("unit.egp")}`,
    rating: `★ ${doctor.averageRating}`,
    "rating@show": Boolean(doctor.averageRating),
    "slot@data-file": doctor.photo ? "" : `doctor-${doctor.id}.jpg`,
    "photo@src": doctor.photo || `img/doctor-${doctor.id}.jpg`,
    "specialty-image@src": `img/spec-${doctor.specialtyId}.jpg`,
    "book@data-id": doctor.id,
    "book@show": canBook,
  });
  return node;
}

function renderDoctorFilters() {
  const select = $("#filter-spec");
  select.replaceChildren(
    new Option(t("filter.all"), ""),
    ...State.specialties.map(specialty => new Option(specialtyLabel(specialty), specialty.id)));
  select.value = State.filters.specialty;
  $("#filter-q").value = State.filters.query;
  $("#filter-available").checked = State.filters.availableToday;
}

function renderDoctorList() {
  const { query, specialty, availableToday } = State.filters;
  const doctors = State.doctors.filter(doctor =>
    isApproved(doctor) &&
    (!specialty || same(doctor.specialtyId, specialty)) &&
    (!availableToday || doctor.availableToday) &&
    (tr(doctor.name) + " " + tr(doctor.city)).toLowerCase().includes(query.trim().toLowerCase()));

  $("#doctor-list").replaceChildren(...doctors.map(doctorCard));
  $("#doctor-empty").hidden = doctors.length > 0;
}

function renderPublicPages() {
  const approved = State.doctors.filter(isApproved);

  $("#stat-doctors").textContent = approved.length;
  $("#stat-specs").textContent = State.specialties.length;

  $("#home-specialties").replaceChildren(...State.specialties.slice(0, 8).map(specialtyCard));
  $("#all-specialties").replaceChildren(...State.specialties.map(specialtyCard));
  $("#home-doctors").replaceChildren(...approved.slice(0, 4).map(doctorCard));

  renderDoctorFilters();
  renderDoctorList();
}

/* ---------- login and register ---------- */
function toggleGroup(fieldset, on) {
  fieldset.hidden = !on;
  fieldset.disabled = !on;
}

/* Date fields: the date of birth cannot be in the future, vacations cannot be in the past */
function setDateLimits() {
  $$("[data-date-of-birth]").forEach(input => { input.max = todayISO(); input.min = "1900-01-01"; });
  $$("[data-future]").forEach(input => { input.min = todayISO(); });
}

function fillSelect(select, options, keepValue = true) {
  const previous = select.value;
  select.replaceChildren(...options);
  if (keepValue) select.value = previous;
}

function renderAuthForms() {
  const registerRole = State.authRole === "assistant" ? "patient" : State.authRole;

  $$('[data-action="auth-role"]').forEach(button => {
    const active = button.closest("form").id === "register-form" ? registerRole : State.authRole;
    button.classList.toggle("is-active", button.dataset.role === active);
    button.setAttribute("aria-pressed", String(button.dataset.role === active));
  });

  toggleGroup($("#register-doctor-fields"), registerRole === "doctor");
  toggleGroup($("#register-patient-fields"), registerRole === "patient");

  fillSelect($("#register-spec"), [
    new Option(t("filter.choose"), ""),
    ...State.specialties.map(specialty => new Option(specialtyLabel(specialty), specialty.id)),
  ]);
  setDateLimits();

  const notice = $("#login-notice");
  notice.hidden = !State.loginNotice;
  notice.textContent = State.loginNotice;
}

/* ---------- patient ---------- */
function renderPatientPages() {
  const me = userId();
  const mine = State.appointments.filter(a => same(a.patientId, me)).sort(byDateTime);

  /* home: next appointment + latest prescriptions */
  const next = mine.find(a => ["pending", "confirmed"].includes(a.status));
  $("#next-appointment").hidden = !next;
  $("#no-next-appointment").hidden = Boolean(next);
  if (next) {
    const doctor = byId(State.doctors, next.doctorId);
    const ticket = $("#next-appointment");
    fill(ticket, {
      date: next.date,
      time: next.time,
      doctor: tr(doctor?.name),
      specialty: specialtyName(doctor?.specialtyId),
    });
    statusChip($('[data-f="status"]', ticket), next.status, t(`status.${next.status}`));
  }

  const prescriptions = State.prescriptions.filter(p => same(p.patientId, me));
  renderList($("#patient-home-prescriptions"), prescriptions.slice(0, 3).map(prescriptionRow));

  /* appointments table */
  const actionsFor = appointment => {
    if (["pending", "confirmed"].includes(appointment.status)) {
      return [actionButton("btn.reschedule", "reschedule", appointment.id),
              actionButton("btn.cancel", "cancel-appointment", appointment.id, "btn-danger")];
    }
    if (appointment.status === "done" && !appointment.reviewed) {
      return [actionButton("btn.rate", "rate", appointment.id)];
    }
    return [];
  };
  renderRows($("#patient-appointments-body"),
    mine.map(a => appointmentRow(a, ["date", "time", "doctor", "type", "fee", "payment", "status", "actions"], actionsFor)), 8);

  /* records page */
  renderList($("#patient-visits"), State.records.filter(r => same(r.patientId, me)).map(r => visitRow(r)));
  renderList($("#patient-prescriptions"), prescriptions.map(prescriptionRow));
  renderList($("#patient-documents"), State.documents.filter(d => same(d.patientId, me)).map(documentRow));
  renderList($("#patient-scans"), State.scans.filter(s => same(s.patientId, me)).map(s => scanRow(s)));
}

/* ---------- doctor ---------- */
function weekDays() {
  const start = addDays(todayISO(), -new Date(todayISO()).getUTCDay());     // Sunday of this week
  return [0, 1, 2, 3, 4].map(i => addDays(start, i));
}

function renderCalendar(appointments) {
  const grid = $("#schedule-grid");
  const locale = State.lang === "ar" ? "ar-EG" : "en-GB";
  const cells = [document.createElement("div")];

  for (const day of weekDays()) {
    const head = document.createElement("div");
    head.className = "week-head";
    head.textContent = new Date(day).toLocaleDateString(locale, { weekday: "short", day: "numeric", timeZone: "UTC" });
    cells.push(head);
  }

  for (const hour of [9, 10, 11, 12, 13, 14]) {
    const label = document.createElement("div");
    label.className = "week-head";
    label.textContent = `${hour}:00`;
    cells.push(label);

    for (const day of weekDays()) {
      const cell = document.createElement("div");
      appointments
        .filter(a => isLive(a) && a.date === day && parseInt(a.time, 10) === hour)
        .forEach(a => {
          const item = document.createElement("span");
          item.className = `week-item${a.status === "pending" ? " is-pending" : ""}`;
          item.textContent = `${a.time} ${patientName(a.patientId)}`;
          cell.append(item);
        });
      cells.push(cell);
    }
  }
  grid.replaceChildren(...cells);
}

function renderWorkingHours() {
  const { hours, vacations } = State.schedule;

  renderRows($("#hours-body"), hours.map(row => {
    const node = cloneTemplate("tpl-hours-row");
    fill(node, {
      day: t(`day.${row.dayOfWeek}`), from: row.startTime, to: row.endTime,
      slot: `${row.slotMinutes} ${t("unit.min")}`,
    });
    statusChip($('[data-f="status"]', node), row.isActive ? "confirmed" : "cancelled", t(row.isActive ? "status.on" : "status.off"));
    $('[data-f="actions"]', node).append(
      actionButton(row.isActive ? "btn.turnOff" : "btn.turnOn", "toggle-hours", row.id),
      actionButton("btn.delete", "delete-hours", row.id, "btn-danger"));
    return node;
  }), 6);

  renderList($("#vacation-list"), vacations.map(vacation => {
    const node = cloneTemplate("tpl-vacation-row");
    fill(node, { dates: `${vacation.startDate} → ${vacation.endDate}`, reason: vacation.reason });
    $('[data-f="actions"]', node).append(actionButton("btn.delete", "delete-vacation", vacation.id, "btn-danger"));
    return node;
  }));
}

function renderScanForm() {
  const me = userId();
  fillSelect($("#scan-patient"), [
    new Option(t("filter.choose"), ""),
    ...State.patients.map(patient => new Option(tr(patient.name), patient.id)),
  ]);
  renderRecordChoices($("#scan-record"), $("#scan-patient").value);

  const scans = State.scans.filter(s => same(s.doctorId, me)).sort((a, b) => b.uploadedAt.localeCompare(a.uploadedAt));
  renderList($("#doctor-scans"), scans.map(scan => scanRow(scan, { showPatient: true, canVerify: true })));
  renderMriResult();
}

/* The visits of one patient (written by the logged-in doctor) as <option>s */
function renderRecordChoices(select, patientId) {
  const records = State.records.filter(r => same(r.patientId, patientId) && (role() !== "doctor" || same(r.doctorId, userId())));
  fillSelect(select, [
    new Option(t("filter.none"), ""),
    ...records.map(r => new Option(`${r.date} – ${r.diagnosis}`, r.id)),
  ]);
}

function renderDoctorPages() {
  const me = userId();
  const mine = State.appointments.filter(a => same(a.doctorId, me)).sort(byDateTime);
  const requests = mine.filter(a => a.status === "pending");
  const needRecord = mine.filter(a => ["confirmed", "done"].includes(a.status) && !a.hasRecord && a.date <= todayISO());
  const rating = byId(State.doctors, me)?.averageRating;

  $("#doctor-stat-today").textContent = mine.filter(a => isLive(a) && a.date === todayISO()).length;
  $("#doctor-stat-requests").textContent = requests.length;
  $("#doctor-stat-confirmed").textContent = mine.filter(a => a.status === "confirmed").length;
  $("#doctor-stat-rating").textContent = rating ? `★ ${rating}` : "–";

  renderRows($("#doctor-requests-body"),
    requests.map(a => appointmentRow(a, ["date", "time", "patient", "type", "status", "actions"], appointment => [
      actionButton("btn.confirm", "confirm-appointment", appointment.id),
      actionButton("btn.reject", "reject-appointment", appointment.id, "btn-danger"),
    ])), 6);

  renderRows($("#doctor-visits-body"),
    needRecord.map(a => appointmentRow(a, ["date", "time", "patient", "type", "status", "actions"],
      appointment => [actionButton("btn.record", "write-record", appointment.id)])), 6);

  renderCalendar(mine);
  renderWorkingHours();

  renderList($("#doctor-patients"), State.patients.map(patient => personRow({
    name: tr(patient.name),
    meta: [`${ageOf(patient)} ${t("unit.years")}`, patient.gender ? t(`gender.${patient.gender}`) : "", patient.bloodType].filter(Boolean).join(" · "),
    actions: [actionButton("btn.patientFile", "patient-file", patient.id)],
  })));

  renderScanForm();
}

/* The doctor's pop-up with everything about one patient */
function renderPatientFile() {
  if (role() !== "doctor" || State.patientFileId == null) return;
  const patient = byId(State.patients, State.patientFileId);
  if (!patient) return;
  const id = patient.id;

  $("#patient-file-name").textContent = tr(patient.name);
  $("#patient-file-info").textContent = [`${ageOf(patient)} ${t("unit.years")}`,
    patient.gender ? t(`gender.${patient.gender}`) : "", patient.bloodType].filter(Boolean).join(" · ");
  $("#patient-file-extra").textContent = [
    patient.allergies ? `${t("label.allergies")}: ${patient.allergies}` : "",
    patient.medicalHistory ? `${t("label.history")}: ${patient.medicalHistory}` : "",
  ].filter(Boolean).join("  |  ");

  const mine = record => same(record.doctorId, userId());
  renderList($("#patient-file-visits"), State.records.filter(r => same(r.patientId, id)).map(record =>
    visitRow(record, mine(record) ? [actionButton("btn.prescription", "write-prescription", record.id)] : [])));
  renderList($("#patient-file-prescriptions"), State.prescriptions.filter(p => same(p.patientId, id)).map(prescriptionRow));
  renderList($("#patient-file-documents"), State.documents.filter(d => same(d.patientId, id)).map(documentRow));
  renderList($("#patient-file-scans"), State.scans.filter(s => same(s.patientId, id)).map(s => scanRow(s)));
}

/* ---------- assistant ---------- */
function renderAssistantPages() {
  const stats = State.stats || {};

  $("#assistant-stat-patients").textContent = (stats.patients ?? State.patients.length).toLocaleString();
  $("#assistant-stat-doctors").textContent = State.doctors.filter(isApproved).length;
  $("#assistant-stat-appointments").textContent = State.appointments.length;
  $("#assistant-stat-revenue").textContent = (stats.revenue ?? 0).toLocaleString();

  /* bars: appointments per month */
  const monthly = stats.monthly || [];
  const highest = Math.max(1, ...monthly.map(m => m.value));
  $("#monthly-bars").replaceChildren(...monthly.map(month => {
    const bar = document.createElement("div");
    bar.className = "bar";
    bar.style.height = `${(month.value / highest) * 100}%`;
    bar.innerHTML = "<em></em><small></small>";
    $("em", bar).textContent = month.value;
    $("small", bar).textContent = tr(month.label);
    return bar;
  }));

  /* donut: share of each specialty */
  const shares = (stats.bySpecialty || []).slice(0, 4);
  const total = shares.reduce((sum, n) => sum + n, 0) || 1;
  let from = 0;
  const slices = shares.map((share, i) => {
    const to = from + (share / total) * 100;
    const slice = `${CHART_COLORS[i]} ${from}% ${to}%`;
    from = to;
    return slice;
  });
  $("#donut").style.background = `conic-gradient(${slices.join(", ") || "var(--line) 0 100%"})`;
  $("#donut-legend").replaceChildren(...State.specialties.slice(0, shares.length).map((specialty, i) => {
    const item = document.createElement("li");
    item.innerHTML = "<i></i><span></span>";
    $("i", item).style.background = CHART_COLORS[i];
    $("span", item).textContent = `${specialtyLabel(specialty)} ${shares[i]}%`;
    return item;
  }));

  renderList($("#assistant-top-doctors"), (stats.topDoctors || []).map(entry => personRow({
    name: doctorName(entry.doctorId),
    meta: `${entry.count} ${t("label.appointments")}`,
    photo: byId(State.doctors, entry.doctorId)?.photo,
  })));

  /* every appointment */
  const actionsFor = appointment => [
    ...(appointment.status === "pending"
      ? [actionButton("btn.confirm", "confirm-appointment", appointment.id),
         actionButton("btn.reject", "reject-appointment", appointment.id, "btn-danger")] : []),
    ...(appointment.paymentStatus !== "Paid" && isLive(appointment)
      ? [actionButton("btn.markPaid", "mark-paid", appointment.id)] : []),
  ];
  renderRows($("#assistant-appointments-body"),
    [...State.appointments].sort(byDateTime).map(a =>
      appointmentRow(a, ["date", "time", "patient", "doctor", "type", "fee", "payment", "status", "actions"], actionsFor)), 9);

  /* doctors waiting for a decision */
  renderList($("#assistant-doctors"), State.doctors.map(doctor => {
    const pending = doctor.approvalStatus === "Pending";
    const rejected = doctor.approvalStatus === "Rejected";
    return personRow({
      name: tr(doctor.name),
      meta: `${specialtyName(doctor.specialtyId)} · ${tr(doctor.city)}`,
      photo: doctor.photo,
      note: rejected && doctor.rejectionReason ? `${t("label.reason")}: ${doctor.rejectionReason}` : "",
      status: t(`approval.${doctor.approvalStatus}`),
      statusClass: doctor.approvalStatus.toLowerCase(),
      actions: [
        ...(pending || rejected ? [actionButton("btn.approve", "approve-doctor", doctor.id)] : []),
        ...(pending ? [actionButton("btn.reject", "reject-doctor", doctor.id, "btn-danger")] : []),
      ],
    });
  }));

  /* patients and their access (USER.IsActive) */
  renderList($("#assistant-patients"), State.patients.map(patient => {
    const active = patient.isActive !== false;
    return personRow({
      name: tr(patient.name),
      meta: `${ageOf(patient)} ${t("unit.years")}`,
      status: t(active ? "status.active" : "status.blocked"),
      statusClass: active ? "confirmed" : "cancelled",
      actions: [actionButton(active ? "btn.block" : "btn.unblock", "toggle-patient-access", patient.id, active ? "btn-danger" : "")],
    });
  }));

  /* specialties */
  renderList($("#assistant-specialties"), State.specialties.map(specialty => {
    const inUse = State.doctors.some(d => same(d.specialtyId, specialty.id));
    return personRow({
      name: specialtyLabel(specialty),
      meta: specialty.description || "",
      photo: specialty.imageUrl || `img/spec-${specialty.id}.jpg`,
      actions: [actionButton("btn.edit", "edit-specialty", specialty.id),
                ...(inUse ? [] : [actionButton("btn.delete", "delete-specialty", specialty.id, "btn-danger")])],
    });
  }));

  /* clinic settings */
  if (State.clinic) {
    const form = $("#clinic-form");
    if (!form.contains(document.activeElement)) {
      for (const field of form.elements) if (field.name && field.name in State.clinic) field.value = State.clinic[field.name] ?? "";
    }
  }
}

/* ---------- profile and notification preferences (every role) ---------- */
function renderProfile() {
  const profile = State.profile;
  if (!profile) return;
  State.profileDraft ??= { photo: profile.photo || "" };

  if (!State.profileDirty) {                                     // do not overwrite what the user is typing
    for (const field of $("#profile-form").elements) {
      if (field.name && field.name in profile) field.value = profile[field.name] ?? "";
    }
  }
  setAvatar($("#profile-avatar"), tr(profile.name), State.profileDraft.photo);
  $("#profile-photo-remove").hidden = !State.profileDraft.photo;

  const prefs = State.preferences;
  const form = $("#preferences-form");
  if (prefs && !form.contains(document.activeElement)) {
    for (const field of form.elements) {
      if (!field.name || !(field.name in prefs)) continue;
      if (field.type === "checkbox") field.checked = Boolean(prefs[field.name]);
      else field.value = prefs[field.name];
    }
  }
}

/* ---------- pop-up contents ---------- */
function renderNotifications() {
  renderList($("#notification-list"), State.notifications.map(notification => {
    const node = cloneTemplate("tpl-notification-row");
    fill(node, {
      title: tr(notification.title),
      message: tr(notification.message),
      time: formatDateTime(notification.createdAt),
    });
    node.classList.toggle("unread", !notification.isRead);
    return node;
  }));
}

async function renderSlots() {
  const { doctorId } = State.booking;
  const date = $("#booking-date").value;
  const slots = date ? await Api.slots(doctorId, date) : [];

  if (!slots.length) {
    const empty = document.createElement("p");
    empty.className = "muted";
    empty.textContent = t("msg.noSlots");
    $("#booking-slots").replaceChildren(empty);
    return;
  }
  $("#booking-slots").replaceChildren(...slots.map(slot => {
    const button = document.createElement("button");
    button.type = "button";
    button.textContent = slot.time;
    button.disabled = !slot.available;
    button.dataset.action = "pick-slot";
    button.dataset.time = slot.time;
    return button;
  }));
}

function renderStars() {
  $$("#rating-stars button").forEach(star => {
    star.classList.toggle("is-on", Number(star.dataset.value) <= (State.rating?.value || 0));
  });
}

function addMedicationRow() {
  $("#medication-rows").append(cloneTemplate("tpl-medication-row"));
}

function prescriptionSheet(prescription) {
  const sheet = document.createElement("div");
  sheet.className = "rx-sheet";

  const title = document.createElement("h2");
  title.textContent = `MediCare – ${t("rx.sheetTitle")}`;

  const info = document.createElement("p");
  info.textContent = `${t("label.patient")}: ${patientName(prescription.patientId) || tr(State.session.user.name)} · ` +
                     `${t("label.doctor")}: ${doctorName(prescription.doctorId)} · ${prescription.date}`;

  const list = document.createElement("ol");
  for (const item of prescription.items) {
    const li = document.createElement("li");
    const strong = document.createElement("strong");
    strong.textContent = `${item.medication} ${item.dosage}`;
    li.append(strong, document.createElement("br"),
      `${item.frequency} · ${item.durationDays} ${t("unit.days")}${item.instructions ? " – " + item.instructions : ""}`);
    list.append(li);
  }
  sheet.append(title, info, list);

  if (prescription.notes) {
    const notes = document.createElement("p");
    notes.textContent = `${t("label.notes")}: ${prescription.notes}`;
    sheet.append(notes);
  }
  return sheet;
}

/* MRI result panel (the latest scan of this doctor) */
function renderMriResult() {
  const scan = byId(State.scans, State.lastScanId);
  $("#mri-result").hidden = !scan;
  $("#mri-empty").hidden = Boolean(scan);
  if (!scan) return;

  const analysis = latestAnalysis(scan);
  const percent = Math.round(analysis.confidence * 100);
  const label = $("#mri-class");
  label.textContent = t(`mri.${analysis.label}`);
  label.className = `mri-class ${analysis.label === "notumor" ? "is-ok" : "is-danger"}`;
  $("#mri-confidence").textContent = `${percent}%`;
  $("#mri-meter").style.width = `${percent}%`;
  $("#mri-model").textContent = analysis.modelVersion || "";

  $("#mri-scores").replaceChildren(...Object.entries(analysis.scores || {}).map(([name, value]) => {
    const row = document.createElement("div");
    row.className = "score-row";
    row.innerHTML = "<span></span><div class='meter'><div></div></div><b></b>";
    $("span", row).textContent = t(`mri.${name}`);
    $(".meter div", row).style.width = `${Math.round(value * 100)}%`;
    $("b", row).textContent = `${Math.round(value * 100)}%`;
    return row;
  }));
}


/* ====================================================================
   8. FORMS AND VALIDATION
   The same rules are written in index.html (required, type, min, max,
   minlength) and checked here again, so the page is safe even if the
   HTML is changed. Repeat these rules in your backend too.
   ==================================================================== */
function checkField(field, form) {
  const wrapper = field.closest("label");
  const label = (wrapper && $(".field-label", wrapper)?.textContent.trim()) || field.placeholder || field.name;

  if (field.type === "file") {
    return field.required && !field.files.length ? `${label} ${t("err.required")}` : "";
  }

  const value = field.value.trim();
  if (field.required && !value) return `${label} ${t("err.required")}`;
  if (!value) return "";

  if (field.type === "email" && !/^[^\s@]+@[^\s@]+\.[^\s@]{2,}$/.test(value)) return t("err.email");
  if (field.type === "tel" && !/^\+?[0-9 ]{10,15}$/.test(value)) return t("err.phone");
  if (field.minLength > 0 && value.length < field.minLength) return t("err.minLength").replace("{n}", field.minLength);

  if (field.type === "number") {
    const number = Number(value);
    if (number < Number(field.min || -Infinity) || number > Number(field.max || Infinity)) return `${label} ${t("err.range")}`;
  }
  if (field.type === "date" || field.type === "time") {
    if ((field.min && value < field.min) || (field.max && value > field.max)) return `${label} ${t("err.range")}`;
  }
  if (field.dataset.after && value <= $(field.dataset.after, form).value) return `${label} ${t("err.after")}`;
  if (field.dataset.notBefore && value < $(field.dataset.notBefore, form).value) return `${label} ${t("err.after")}`;
  if (field.dataset.match && value !== $(field.dataset.match, form).value) return t("err.match");
  return "";
}

/* Returns the first error message, or "" when the form is valid */
function validateForm(form) {
  for (const field of $$("input, select, textarea", form)) {
    if (field.disabled || field.closest("[hidden]")) continue;
    const message = checkField(field, form);
    if (message) {
      field.setAttribute("aria-invalid", "true");
      field.focus();
      return message;
    }
    field.removeAttribute("aria-invalid");
  }
  return "";
}

function showFormError(form, message) {
  $(".form-error", form).textContent = message;
}

function errorMessage(error) {
  const keys = { bad: "err.badLogin", pending: "err.pending", rejected: "err.rejected", exists: "err.exists", taken: "err.taken", blocked: "err.blocked" };
  return t(keys[error.message] || "err.generic");
}

/* Run a backend call, then reload the data and redraw */
async function perform(task) {
  try {
    await task();
    await refreshData();
    renderAll();
    return true;
  } catch (error) {
    toast(errorMessage(error));
    return false;
  }
}

/* A form that sends something to the backend:
   validate -> call the API -> reload -> redraw -> (optional) close the pop-up  */
function submitter({ modal, then, keepValues = false }, send) {
  return async form => {
    const problem = validateForm(form);
    if (problem) return showFormError(form, problem);
    showFormError(form, "");
    try {
      const result = await send(form, Object.fromEntries(new FormData(form)));
      if (!keepValues) form.reset();
      if (modal) closeModal(modal);
      await refreshData();
      renderAll();
      then?.(result);
    } catch (error) {
      showFormError(form, errorMessage(error));
    }
  };
}

const FORM_HANDLERS = {
  async "login-form"(form) {
    const problem = validateForm(form);
    if (problem) return showFormError(form, problem);

    const data = Object.fromEntries(new FormData(form));
    try {
      const result = await Api.login(data.email.trim(), data.password, State.authRole);
      State.loginNotice = "";
      form.reset();
      await startSession(result);
    } catch (error) {
      showFormError(form, errorMessage(error));
    }
  },

  async "register-form"(form) {
    const problem = validateForm(form);
    if (problem) return showFormError(form, problem);

    const registerRole = State.authRole === "assistant" ? "patient" : State.authRole;
    const data = Object.fromEntries(new FormData(form));      // disabled (hidden) fields are not included
    delete data.confirm;

    try {
      const result = await Api.register({ ...data, role: registerRole });
      form.reset();
      if (result?.pending) {
        State.loginNotice = t("msg.pendingDoctor");
        return goTo("login");
      }
      await startSession(await Api.login(data.email.trim(), data.password, registerRole));
    } catch (error) {
      showFormError(form, errorMessage(error));
    }
  },

  async "profile-form"(form) {
    const problem = validateForm(form);
    if (problem) return showFormError(form, problem);
    showFormError(form, "");

    const data = Object.fromEntries(new FormData(form));
    data.photo = State.profileDraft?.photo || "";
    try {
      await Api.saveProfile(data);
      State.session.user.name = data.name;                      // keep the header in sync
      storage.set(CONFIG.SESSION_KEY, JSON.stringify(State.session));
      State.profileDirty = false;
      State.profileDraft = null;
      await refreshData();
      renderAll();
      toast(t("msg.saved"));
    } catch (error) {
      showFormError(form, errorMessage(error));
    }
  },

  async "preferences-form"(form) {
    const problem = validateForm(form);
    if (problem) return showFormError(form, problem);
    showFormError(form, "");
    const get = name => form.elements[name];
    try {
      await Api.savePreferences({
        inAppEnabled: get("inAppEnabled").checked, emailEnabled: get("emailEnabled").checked,
        smsEnabled: get("smsEnabled").checked, reminderHoursBefore: Number(get("reminderHoursBefore").value),
      });
      await refreshData();
      renderAll();
      toast(t("msg.saved"));
    } catch (error) {
      showFormError(form, errorMessage(error));
    }
  },

  /* doctor: working hours and vacations */
  "hours-form": submitter({}, (form, data) =>
    Api.addHours({ ...data, dayOfWeek: Number(data.dayOfWeek), slotMinutes: Number(data.slotMinutes) })),
  "vacation-form": submitter({}, (form, data) => Api.addVacation(data)),

  /* doctor: visit record, prescription, documents, MRI */
  "record-form": submitter({ modal: "modal-record" }, (form, data) =>
    Api.createRecord({ appointmentId: State.recordAppointmentId, ...data })),

  "prescription-form": submitter({ modal: "modal-prescription", then: result => openPrescriptionView(result.id) }, (form, data) => {
    const items = $$(".medication-row", form).map(row => ({
      medication: $('[name="medication"]', row).value.trim(),
      dosage: $('[name="dosage"]', row).value.trim(),
      frequency: $('[name="frequency"]', row).value.trim(),
      durationDays: Number($('[name="durationDays"]', row).value),
      instructions: $('[name="instructions"]', row).value.trim(),
    }));
    return Api.addPrescription({ recordId: State.prescriptionRecordId, notes: data.notes || "", items });
  }),

  "document-form": submitter({ modal: "modal-document" }, form => {
    const body = new FormData(form);
    body.set("patientId", State.documentPatientId);
    if (!body.get("recordId")) body.delete("recordId");
    return Api.uploadDocument(body);
  }),

  "scan-form": submitter({ then: scan => { State.lastScanId = scan.id; resetMriPreview(); renderAll(); toast(t("msg.analysed")); } }, form => {
    const body = new FormData(form);
    if (!body.get("recordId")) body.delete("recordId");
    return Api.createScan(body);
  }),

  "analysis-form": submitter({ modal: "modal-analysis-review" }, (form, data) =>
    Api.reviewAnalysis(State.analysisId, { status: data.status, doctorNotes: data.doctorNotes.trim() })),

  /* patient */
  async "rating-form"(form) {
    if (!State.rating?.value) return showFormError(form, t("msg.chooseRating"));
    const comment = new FormData(form).get("comment") || "";
    closeModal("modal-rating");
    if (await perform(() => Api.review({ appointmentId: State.rating.appointmentId, rating: State.rating.value, comment }))) {
      toast(t("msg.thanks"));
    }
  },

  /* a reason is needed to cancel / reject */
  "reason-form": submitter({ modal: "modal-reason" }, (form, data) => {
    const { kind, id } = State.reason;
    const reason = data.reason.trim();
    if (kind === "reject-doctor") return Api.decideDoctor(id, "Rejected", reason);
    return Api.setStatus(id, kind === "cancel-appointment" ? "cancelled" : "rejected", reason);
  }),

  /* assistant */
  "clinic-form": submitter({ keepValues: true }, (form, data) =>
    Api.saveClinic({ ...data, defaultSlotMinutes: Number(data.defaultSlotMinutes) }).then(() => toast(t("msg.saved")))),

  "specialty-form": submitter({ modal: "modal-specialty" }, (form, data) => Api.saveSpecialty(data, State.editSpecialtyId)),
};

document.addEventListener("submit", event => {
  const handler = FORM_HANDLERS[event.target.id];
  if (!handler) return;
  event.preventDefault();
  handler(event.target);
});


/* ====================================================================
   9. BUTTONS AND POP-UPS
   Every clickable thing in index.html has data-action="name".
   Add a new button = add its name in ACTIONS below.
   ==================================================================== */

/* ---- booking ---- */
async function openBooking(doctorId, rescheduleId = null) {
  if (role() !== "patient") {
    toast(t("msg.loginFirst"));
    return goTo("login");
  }
  const doctor = byId(State.doctors, doctorId);
  State.booking = { doctorId, rescheduleId };

  $("#booking-title").textContent = tr(doctor.name);
  $("#booking-subtitle").textContent = `${specialtyName(doctor.specialtyId)} · ${tr(doctor.city)}`;
  $("#booking-details").hidden = Boolean(rescheduleId);        // type and notes only when booking
  $("#booking-notes-field").hidden = Boolean(rescheduleId);
  const date = $("#booking-date");
  date.min = todayISO();
  date.value = addDays(todayISO(), 1);

  await renderSlots();
  openModal("modal-booking");
}

async function confirmSlot(time) {
  const { doctorId, rescheduleId } = State.booking;
  const date = $("#booking-date").value;
  closeModal("modal-booking");

  const done = await perform(() => rescheduleId
    ? Api.reschedule(rescheduleId, { date, time })
    : Api.book({ doctorId, date, time, type: $("#booking-type").value, notes: $("#booking-notes").value.trim() }));
  if (done) {
    toast(t("msg.booked"));
    $("#booking-notes").value = "";
  }
}

/* ---- small pop-ups ---- */
function openReason(kind, id) {
  State.reason = { kind, id };
  const titles = { "cancel-appointment": "reason.cancel", "reject-appointment": "reason.rejectAppointment", "reject-doctor": "reason.rejectDoctor" };
  $("#reason-title").textContent = t(titles[kind]);
  $("#reason-form").reset();
  showFormError($("#reason-form"), "");
  openModal("modal-reason");
}

function openRating(appointmentId) {
  State.rating = { appointmentId, value: 0 };
  $("#rating-form").reset();
  showFormError($("#rating-form"), "");
  renderStars();
  openModal("modal-rating");
}

function openRecordForm(appointmentId) {
  const appointment = byId(State.appointments, appointmentId);
  State.recordAppointmentId = appointmentId;
  $("#record-patient").textContent = `${patientName(appointment.patientId)} · ${appointment.date}`;
  $("#record-form").reset();
  showFormError($("#record-form"), "");
  openModal("modal-record");
}

function openPrescriptionForm(recordId) {
  const record = byId(State.records, recordId);
  State.prescriptionRecordId = recordId;
  $("#prescription-title").textContent = `${patientName(record.patientId)} · ${record.date}`;
  $("#prescription-form").reset();
  $("#medication-rows").replaceChildren();
  addMedicationRow();
  showFormError($("#prescription-form"), "");
  openModal("modal-prescription");
}

function openPrescriptionView(id) {
  const prescription = byId(State.prescriptions, id);
  if (!prescription) return;
  $("#prescription-sheet").replaceChildren(prescriptionSheet(prescription));
  openModal("modal-prescription-view");
}

function openDocumentUpload(patientId) {
  State.documentPatientId = patientId;
  const form = $("#document-form");
  form.reset();
  $("#document-patient-field").hidden = true;                  // the patient is already known
  $("#document-patient").disabled = true;
  renderRecordChoices($("#document-record"), patientId);
  showFormError(form, "");
  openModal("modal-document");
}

function openPatientFile(patientId) {
  State.patientFileId = patientId;
  renderPatientFile();
  openModal("modal-patient-file");
}

function openAnalysisReview(analysisId) {
  State.analysisId = analysisId;
  const scan = State.scans.find(s => s.analyses.some(a => same(a.id, analysisId)));
  const analysis = scan.analyses.find(a => same(a.id, analysisId));
  $("#analysis-summary").textContent =
    `${patientName(scan.patientId)} · ${t(`mri.${analysis.label}`)} · ${Math.round(analysis.confidence * 100)}%`;
  $("#analysis-form").reset();
  showFormError($("#analysis-form"), "");
  openModal("modal-analysis-review");
}

function openSpecialtyForm(id = null) {
  State.editSpecialtyId = id;
  const form = $("#specialty-form");
  form.reset();
  const specialty = id == null ? null : byId(State.specialties, id);
  if (specialty) {
    for (const field of form.elements) if (field.name) field.value = (field.name === "name" ? tr(specialty.name) : specialty[field.name]) ?? "";
  }
  showFormError(form, "");
  openModal("modal-specialty");
}

async function openNotifications() {
  renderNotifications();
  openModal("modal-notifications");
  const unread = State.notifications.filter(n => !n.isRead);
  await Promise.all(unread.map(n => Api.markNotificationRead(n.id).catch(() => {})));
  unread.forEach(n => { n.isRead = true; });                   // the list above still shows them as new
  renderHeader();
}

/* ---- pictures and files ---- */
function handleProfilePhoto(file) {
  if (!file) return;
  const reader = new FileReader();
  reader.onload = () => {
    const image = new Image();
    image.onload = () => {
      const canvas = document.createElement("canvas");
      const scale = Math.min(1, 320 / Math.max(image.width, image.height));
      canvas.width = image.width * scale;
      canvas.height = image.height * scale;
      canvas.getContext("2d").drawImage(image, 0, 0, canvas.width, canvas.height);
      State.profileDraft = { photo: canvas.toDataURL("image/jpeg", 0.85) };
      State.profileDirty = true;
      renderProfile();
    };
    image.src = reader.result;
  };
  reader.readAsDataURL(file);
}

function previewMri(file) {
  if (!file) return;
  const reader = new FileReader();
  reader.onload = () => {
    const preview = $("#mri-preview");
    preview.src = reader.result;
    preview.hidden = false;
    $("#mri-placeholder").hidden = true;
  };
  reader.readAsDataURL(file);
}

function resetMriPreview() {
  $("#mri-preview").hidden = true;
  $("#mri-placeholder").hidden = false;
}

/* ---- every data-action ---- */
const ACTIONS = {
  "toggle-lang": () => {
    State.lang = State.lang === "ar" ? "en" : "ar";
    storage.set("lang", State.lang);
    applyLanguageAndTheme();
    renderAll();
  },
  "toggle-theme": () => {
    State.theme = State.theme === "dark" ? "light" : "dark";
    storage.set("theme", State.theme);
    applyLanguageAndTheme();
  },
  "toggle-account-menu": button => {
    const menu = $("#account-menu");
    menu.hidden = !menu.hidden;
    button.setAttribute("aria-expanded", String(!menu.hidden));
  },
  "logout":             (_, event) => { event.preventDefault(); signOut(); },
  "open-notifications": openNotifications,
  "close-modal":        button => button.closest("dialog").close(),

  "auth-role":        button => { State.authRole = button.dataset.role; renderAuthForms(); },
  "filter-specialty": link => { State.filters.specialty = link.dataset.id; },

  /* appointments */
  "book":                button => openBooking(button.dataset.id),
  "pick-slot":           button => confirmSlot(button.dataset.time),
  "reschedule":          button => openBooking(byId(State.appointments, button.dataset.id).doctorId, button.dataset.id),
  "cancel-appointment":  button => openReason("cancel-appointment", button.dataset.id),
  "reject-appointment":  button => openReason("reject-appointment", button.dataset.id),
  "confirm-appointment": button => perform(() => Api.setStatus(button.dataset.id, "confirmed")),
  "mark-paid":           button => perform(() => Api.markPaid(button.dataset.id)),
  "rate":                button => openRating(button.dataset.id),
  "set-rating":          button => { State.rating.value = Number(button.dataset.value); renderStars(); },

  /* doctor */
  "write-record":        button => openRecordForm(button.dataset.id),
  "patient-file":        button => openPatientFile(button.dataset.id),
  "write-prescription":  button => openPrescriptionForm(button.dataset.id),
  "add-medication":      addMedicationRow,
  "view-prescription":   button => openPrescriptionView(button.dataset.id),
  "print-prescription":  () => {
    $("#print-area").replaceChildren($("#prescription-sheet").firstElementChild.cloneNode(true));
    window.print();
  },
  "upload-document":     () => openDocumentUpload(role() === "patient" ? userId() : State.patientFileId),
  "verify-analysis":     button => openAnalysisReview(button.dataset.id),
  "toggle-hours":        button => {
    const row = byId(State.schedule.hours, button.dataset.id);
    perform(() => Api.setHoursActive(row.id, !row.isActive));
  },
  "delete-hours":        button => perform(() => Api.deleteHours(button.dataset.id)),
  "delete-vacation":     button => perform(() => Api.deleteVacation(button.dataset.id)),

  /* assistant */
  "approve-doctor":        button => perform(() => Api.decideDoctor(button.dataset.id, "Approved")),
  "reject-doctor":         button => openReason("reject-doctor", button.dataset.id),
  "toggle-patient-access": button => {
    const patient = byId(State.patients, button.dataset.id);
    perform(() => Api.setPatientAccess(patient.id, patient.isActive === false));
  },
  "add-specialty":         () => openSpecialtyForm(),
  "edit-specialty":        button => openSpecialtyForm(button.dataset.id),
  "delete-specialty":      button => { if (confirm(t("msg.confirmDelete"))) perform(() => Api.deleteSpecialty(button.dataset.id)); },

  /* profile */
  "remove-photo": () => { State.profileDraft = { photo: "" }; State.profileDirty = true; renderProfile(); },
};

document.addEventListener("click", event => {
  const trigger = event.target.closest("[data-action]");
  if (trigger) ACTIONS[trigger.dataset.action]?.(trigger, event);

  // the account menu closes when you click anywhere else (or on one of its links)
  const menu = $("#account-menu");
  if (!menu.hidden && (!event.target.closest(".account") || event.target.closest(".account-menu a"))) {
    menu.hidden = true;
  }
});

document.addEventListener("input", event => {
  const field = event.target;
  if (field.closest("form")) {                      // typing fixes the problem: hide the old error
    field.removeAttribute("aria-invalid");
    const message = $(".form-error", field.closest("form"));
    if (message) message.textContent = "";
  }
  if (field.id === "filter-q") { State.filters.query = field.value; renderDoctorList(); }
  if (field.closest("#profile-form")) State.profileDirty = true;
});

document.addEventListener("change", event => {
  const field = event.target;
  if (field.id === "filter-spec")          { State.filters.specialty = field.value; renderDoctorList(); }
  if (field.id === "filter-available")     { State.filters.availableToday = field.checked; renderDoctorList(); }
  if (field.id === "booking-date")         renderSlots();
  if (field.id === "mri-file")             previewMri(field.files[0]);
  if (field.id === "scan-patient")         renderRecordChoices($("#scan-record"), field.value);
  if (field.id === "profile-photo-input")  handleProfilePhoto(field.files[0]);
});

window.addEventListener("hashchange", () => { renderAll(); window.scrollTo(0, 0); });


/* ====================================================================
   10. START
   ==================================================================== */
async function start() {
  State.lang  = storage.get("lang")  || (navigator.language.startsWith("ar") ? "ar" : "en");
  State.theme = storage.get("theme") || (matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light");

  try { State.session = JSON.parse(storage.get(CONFIG.SESSION_KEY)); } catch { State.session = null; }

  applyLanguageAndTheme();
  markMissingPhotos();
  try {
    await refreshData();
    startRealtime();
  } catch {
    if (State.session) await signOut();          // the saved session is no longer valid
  }
  renderAll();
}

/* Wait until every <script> (including the optional mock server) has loaded */
if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", start);
else start();
