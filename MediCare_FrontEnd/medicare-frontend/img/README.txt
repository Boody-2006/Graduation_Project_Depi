Put your real images in this folder with EXACTLY these names (jpg).
While a file is missing, the website shows a soft placeholder with the file name written on it,
so you always know which name to use. Nothing breaks if an image is missing.

PAGE IMAGES
  hero.jpg        home page, top section              (landscape, about 1200x900)
  ai.jpg          home page, AI brain-MRI section      (landscape, about 1200x900)
  about.jpg       About page                           (landscape, about 1200x900)
  auth.jpg        beside the login / register forms    (landscape, about 1200x900)

SPECIALTY IMAGES (one per specialty id; shown on cards and as a small icon next to each doctor)
  spec-0.jpg   Neurology            spec-4.jpg   Cardiology
  spec-1.jpg   Radiology            spec-5.jpg   Pediatrics
  spec-2.jpg   Neurosurgery         spec-6.jpg   Dermatology
  spec-3.jpg   Internal medicine    spec-7.jpg   Orthopedics
  (spec-N.jpg where N = the specialization id coming from your API; landscape 3:2, about 900x600)

DOCTOR PHOTOS (square, about 600x600)
  doctor-0.jpg, doctor-1.jpg, doctor-2.jpg ... doctor-N.jpg  (N = doctor id)
  Or return a "photo" URL from the API. Doctors can also upload their own photo from their profile.
