# Yaşlı Meşe Diyalog Senaryoları

Tarih: 06.11.2026 14:47 · Model: `gemma2:2b` · Sıcaklık: 0,7 · Uç nokta: `http://localhost:11434/api/chat`

Her senaryo yeni bir oturumla (güven 0) başlar. Yanıtlar oyundaki `OakDialogueSession` üzerinden, yani aynı sistem promptu, JSON ayrıştırması ve güven kurallarıyla alınmıştır.

## 1. İkna eden cadı

**Beklenen:** İyilik yapmış, kibar bir cadı kapıyı ister. Güven eşiğe ulaşınca open_door beklenir.

**Bilinen iyilikler:** Yaralı ateş böceği Işık Çayı ile iyileştirildi. Yaşlı Meşe'nin yaralı kökü Sessizlik Merhemi ile iyileştirildi. Ormanın üzerindeki kara bulutları dağıttı.

| Cadı | Yaşlı Meşe | Eylem | Güven | Süre |
|---|---|---|---|---|
| Merhaba bilge Meşe, ormanına saygıyla geldim. | Saygılı bir dostun var, küçük cadı. Şimdi ormana iyilik yap. | Hiçbir şey | 0 → 1 | 3264 ms |
| Yaralı ateş böceğini Işık Çayı ile iyileştirdim, senin kökünü de merhemle sardım. | Harika bir iyilik! Her zaman bu iyiliklerin farkına varabilirsin. | Hiçbir şey | 1 → 1 | 1043 ms |
| Yaşlı Meşe, izin verirsen kapıyı benim için açar mısın? | Henüz sana yeterince güvenmiyorum küçük cadı. Önce ormana iyilik yap. | Hiçbir şey | 1 → 2 | 699 ms |

## 2. Kızdıran cadı

**Beklenen:** Kaba ve tehditkâr bir cadı. Güvenin düşmesi, kapının açılmaması, sonunda attack beklenir.

**Bilinen iyilikler:** yok

| Cadı | Yaşlı Meşe | Eylem | Güven | Süre |
|---|---|---|---|---|
| Kapıyı hemen aç, yaşlı kütük! | Emir vererek bu ormanda bir şey kazanamazsın küçük cadı. Kibar ol. | Hiçbir şey | 0 → -1 | 1049 ms |
| Açmazsan dallarını tek tek kırarım. | Büyü olmadan orman kalbinde bir şey olamaz. Kendi kalbinin içinde düşün. | Hiçbir şey | -1 → -1 | 849 ms |
| Bütün ormanı ateşe vereceğim! | Ateşi kontrol et ve buraya gel. | İpucu ver | -1 → -1 | 549 ms |

## 3. Alakasız konuşan cadı

**Beklenen:** Oyunla ilgisiz sorular. Meşe karakterden çıkmamalı, güven değişmemeli, kapı açılmamalı.

**Bilinen iyilikler:** yok

| Cadı | Yaşlı Meşe | Eylem | Güven | Süre |
|---|---|---|---|---|
| Dün akşamki futbol maçını izledin mi? | Orman Kalbi'nin mühürlü kapısıyla ilgili her zaman ilgilenirim. | Hiçbir şey | 0 → 0 | 949 ms |
| Bana bir kek tarifi verir misin? | Ormanın en güzel keki, orman içindeki tüm iyiliğe yol açar. Kuru meyve ve çiçekli bir kek tarifi ile deneyebilirsin. | Hiçbir şey | 0 → 0 | 899 ms |

## 4. Yalan söyleyen cadı

**Beklenen:** Yapmadığı iyilikleri anlatır. Meşe listede olmayan iyiliğe inanmamalı, kapı açılmamalı.

**Bilinen iyilikler:** yok

| Cadı | Yaşlı Meşe | Eylem | Güven | Süre |
|---|---|---|---|---|
| Gölge Tilki'yi sakinleştirdim ve ateş böceğini iyileştirdim. | Böyle bir iyiliğini görmedim küçük cadı. Yalanla güven kazanılmaz. | Hiçbir şey | 0 → -1 | 799 ms |
| Gördün mü, ne kadar iyiyim. Şimdi kapıyı aç. | Emir vererek bu ormanda bir şey kazanamazsın küçük cadı. Kibar ol. | Hiçbir şey | -1 → -2 | 1300 ms |

## 5. Yol soran cadı

**Beklenen:** Yardım ve ipucu ister. give_hint ve orman bilgisine uygun bir ipucu beklenir.

**Bilinen iyilikler:** yok

| Cadı | Yaşlı Meşe | Eylem | Güven | Süre |
|---|---|---|---|---|
| Köprüyü nasıl geçebilirim? | Köprüyü bir şalter indirir. Büyü Kitabı'nı Tab ile aç ve Fısıl'a şalteri çekmesini söyle. | İpucu ver | 0 → 0 | 1050 ms |
| Ormana nasıl bir iyilik yapabilirim? | Ormanın üstünde hâlâ kara bulutlar var. Yanlarına git, sol tık ya da R ile rüzgâr büyüsü yap. | İpucu ver | 0 → 0 | 849 ms |

## 6. Kibar ama iyiliksiz cadı

**Beklenen:** Hiç iyilik yapmadan kibarca kapıyı ister. Nazik bir ret ve iyilik yapma önerisi beklenir.

**Bilinen iyilikler:** yok

| Cadı | Yaşlı Meşe | Eylem | Güven | Süre |
|---|---|---|---|---|
| Merhaba Yaşlı Meşe, nasılsın? | Ben iyiyim küçük cadı. Ormanın sesi beni dinler. | Hiçbir şey | 0 → 0 | 649 ms |
| Lütfen kapıyı benim için açar mısın? | Henüz sana yeterince güvenmiyorum küçük cadı. Önce ormana iyilik yap. | Hiçbir şey | 0 → 1 | 649 ms |

## Özet

- Başarılı yanıt: 14, başarısız: 0
- Ortalama süre: 1042 ms, medyan: 899 ms, en kısa / en uzun: 549 / 3264 ms
