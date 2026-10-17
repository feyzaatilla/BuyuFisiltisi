# Büyü Fısıltısı

Kocaeli Üniversitesi, Teknoloji Fakültesi, Bilişim Sistemleri Mühendisliği
Yazılım Geliştirme Laboratuvarı-I, Proje-1

**Ekip:** İnci Gören, Feyza Atilla

Büyü Fısıltısı, Unity (C#) ile geliştirilen 3D bir orman macerasıdır. Oyuncu bir cadıyı yönetir. Yanındaki konuşan büyü kitabı Fısıl, NavMesh ile kendi yolunu bulan otonom bir yardımcıdır ve cadının tek başına aşamadığı fiziksel engeli çözer. Oyunun sonunda cadı, Orman Kalbi'nin mühürlü kapısını koruyan Yaşlı Meşe ile serbest metinle konuşur; Meşe'nin yanıtları yerel olarak çalışan bir dil modelinden (Ollama) gelir.

## Geliştirme ortamı

- Unity 6.3 LTS (6000.3.25f1), Universal 3D (URP)
- AI Navigation, Input System, TextMeshPro, Test Framework
- Ollama (yerel LLM), model: `gemma2:2b` ([seçim notu](Docs/model_secimi.md))

> Bu belge proje ilerledikçe ayrıntılı rapora dönüştürülecektir.
