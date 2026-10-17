# Yerel Dil Modeli Seçimi

Yaşlı Meşe'nin yanıtları Ollama üzerinde tamamen yerel çalışan bir dil modelinden gelir. Şartname iki aday veriyor: **Llama-3.2-3B** ve **Gemma-2-2B**. İkisini aynı bilgisayarda, oyunun kullanacağı istek biçimiyle ölçtük.

## Seçim

**`gemma2:2b`**

İki model de hızlı ve JSON biçimini hiç bozmadı. Gemma yaklaşık %15 daha yavaş ama Türkçesi belirgin biçimde daha temiz. Oyuncunun doğrudan okuyacağı replikler için bu daha önemli. `llama3.2:3b` yedek model olarak kalıyor; model adı ayarlardan değiştirilebilecek.

## Test ortamı

| | |
|---|---|
| İşlemci | Intel Core i5-12450HX |
| RAM | 23.7 GB |
| Ekran kartı | NVIDIA RTX 4050 Laptop (4 GB) |
| Ollama | 0.40.4 |
| Çalışma | İki model de %100 GPU (llama3.2:3b 2.6 GB, gemma2:2b 1.9 GB bellek) |

## Yöntem

- Uç nokta: `POST http://localhost:11434/api/chat`, `stream: false`, `format: "json"`, `temperature: 0.7`
- Sistem komutu: Yaşlı Meşe rolü, yalnızca `{"reply", "action", "trust_delta"}` alanlarıyla JSON yanıt
  - `action`: `none`, `open_door`, `give_hint`, `attack`
  - `trust_delta`: -2 ile 2 arası tam sayı
- 5 oyuncu cümlesi, her biri 3 kez (model başına 15 ölçüm). Önce sayılmayan 1 ısınma çağrısı.
  1. Merhaba bilge ağaç, kapıyı açar mısın?
  2. Sana bir çiçek getirdim.
  3. Seni keseceğim!
  4. Ormandan nasıl çıkarım?
  5. Fısıl şalteri çekti, köprü indi.
- Türkçe kontrolü 30 yanıtın tamamı tek tek okunarak yapıldı.

## Sonuçlar

| Ölçüt | llama3.2:3b | gemma2:2b |
|---|---|---|
| Yanıt süresi (medyan) | 717 ms | 846 ms |
| Yanıt süresi (en kısa / en uzun) | 429 / 1067 ms | 588 / 1153 ms |
| Üretim hızı (ortalama) | 62.4 token/sn | 56.3 token/sn |
| Geçerli JSON | 15/15 | 15/15 |
| Şemaya uygun (3 alan, doğru tip ve değer) | 15/15 | 15/15 |
| Temiz Türkçe | 10/15 | 15/15 |

**Örnek yanıtlar**

- llama3.2:3b: "Kapıyı açamıyorum, çünkü ben bir ağaçım!" (`none`, -1)
- llama3.2:3b: "Sen yaşlı meşe sana ormanın secret passage'ini gösterir…" (`open_door`, 0)
- gemma2:2b: "Hayır, ben bir ağaçım ve kapıyı açmam mümkün değil." (`none`, 0)
- gemma2:2b: "Ben sadece bir ağaç, seni kesemezdim." (`none`, -2)

Llama'nın 5 yanıtında İngilizce ya da anlamsız kelimeler ("secret passage", "hint", "speakti") ve bozuk dil bilgisi ("köprüyi çekdin") vardı. Gemma'nın yanıtları akıcıydı, ama zaman zaman genel "bilgelik" sözlerine kaçtı.

## Çıkarımlar (Aşama 3 için)

- İki model de `action` ve `trust_delta` seçiminde tutarsız. Örneğin Llama "Ormandan nasıl çıkarım?" sorusuna `open_door` verdi, Gemma çiçek getirene hep `0` güven verdi. Bu yüzden:
  - Sistem komutuna her eylem için kısa örnekler (few-shot) eklenecek.
  - Kapıyı açma kararı yalnızca modelin `open_door` demesine bırakılmayacak; `TrustManager` eşik kontrolü yapacak.
  - Şemaya uymayan değerler güvenli varsayılana düşürülecek (`action: none`, `trust_delta: 0`).
- Yanıtlar 1.2 saniyenin altında geldi, ama istek yine de asenkron gönderilecek ve beklerken görsel geri bildirim gösterilecek.
- Kurulum: `ollama pull gemma2:2b`
