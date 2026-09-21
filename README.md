# Gacha Calculators — cài trên Windows và điện thoại

Bộ này gồm 5 app riêng biệt. Mỗi app cài độc lập và lưu dữ liệu riêng:

```
calculators/
  index.html        ← trang tổng, liệt kê 5 app
  genshin/          ← Nguyên Thạch Calculator (Genshin Impact)
  hsr/              ← Tinh Ngọc Calculator (Honkai: Star Rail)
  hi3/              ← Pha Lê Calculator (Honkai Impact 3rd)
  zzz/              ← Polychrome Calculator (Zenless Zone Zero)
  wuwa/             ← Astrite Calculator (Wuthering Waves)
```

Quan trọng: app **phải chạy qua http/https**, không mở bằng cách bấm đúp vào file. Mở kiểu `file://` thì trình duyệt chặn cài đặt và chặn chạy offline.

---

## Thông số gacha mặc định

| Game | Giá 1 lượt | Bảo hiểm banner nhân vật | 50/50 |
|---|---|---|---|
| Genshin Impact | 160 Nguyên Thạch | 90 | Có |
| Honkai: Star Rail | 160 Tinh Ngọc | 90 | Có |
| Honkai Impact 3rd | 280 Pha Lê | tuỳ banner, bạn tự điền | Không |
| Zenless Zone Zero | 160 Polychrome | 90 (S-Rank Agent) | Có |
| Wuthering Waves | 160 Astrite | 80 (5★ Resonator) | Có |

Banner vũ khí có luật riêng: ZZZ 80 lượt với tỉ lệ 75/25, WuWa 80 lượt và không có 50/50, Genshin và HSR 80 lượt. Khi tính cho banner vũ khí, sửa ô "Lượt bảo hiểm tối đa" cho đúng.

Danh sách nguồn thu mặc định chỉ là điểm khởi đầu theo mức F2P thông thường. Sự kiện và phần thưởng thay đổi mỗi phiên bản, nên hãy sửa lại theo tài khoản của bạn — app sẽ nhớ.

---

## Bước 1 — Đưa lên mạng bằng GitHub Pages (khuyên dùng)

Cách này cho link https thật, dùng được cho cả laptop lẫn điện thoại, và không phải bật gì mỗi lần dùng.

1. Tạo một repo mới trên GitHub, ví dụ `gacha-calculators`, đặt Public.
2. Upload toàn bộ nội dung thư mục `calculators` vào repo (kéo thả thẳng trên web cũng được).
3. Vào **Settings → Pages**, mục Source chọn **Deploy from a branch**, branch `main`, folder `/ (root)`, bấm Save.
4. Đợi 1–2 phút, link sẽ là `https://<tên-github>.github.io/gacha-calculators/`.

Netlify hoặc Cloudflare Pages cũng được, chỉ cần kéo thả thư mục lên.

**Nếu bạn đã đưa bản zip cũ lên:** hãy thay bằng bản này. Bản cũ có lỗi khiến Genshin, ZZZ và WuWa ghi đè dữ liệu của nhau khi chung một địa chỉ web.

Chỉ muốn thử nhanh trên laptop, không cần internet: bấm đúp `run-local.bat` (cần Python, nhớ tích **Add Python to PATH** khi cài), rồi mở `http://localhost:8080/`. Cách này không dùng được cho điện thoại, và phải giữ cửa sổ đen mở.

---

## Bước 2 — Cài trên laptop (Chrome hoặc Edge)

1. Mở link ở bước 1, chọn app muốn dùng.
2. Bấm nút **Cài app** trong app, hoặc icon cài đặt (màn hình có mũi tên xuống) trên thanh địa chỉ.

App có icon riêng trong Start Menu, mở ra cửa sổ riêng không thanh địa chỉ và chạy được khi mất mạng. Cài app nào chỉ cài app đó.

## Bước 2 — Cài trên điện thoại

**Android (Chrome):** mở link, bấm nút **Cài app** trong app, hoặc menu ⋮ → **Cài đặt ứng dụng** / **Thêm vào màn hình chính**.

**iPhone / iPad:** phải dùng **Safari** (Chrome trên iOS không cài được). Bấm nút **Chia sẻ** (ô vuông có mũi tên lên) → **Thêm vào MH chính**.

Lưu ý riêng cho iPhone:

- Bản cài trên màn hình chính có dữ liệu **tách biệt** với tab Safari. Hãy nhập liệu sau khi đã cài, hoặc chuyển từ máy khác sang bằng mã (xem bên dưới).
- Tab Safari thường có thể bị iOS xoá dữ liệu nếu lâu không mở. App đã thêm vào màn hình chính không bị quy tắc đó. Dù vậy, thỉnh thoảng bạn nên bấm **Sao chép mã** và dán vào Ghi chú để dự phòng.

Cài xong, mỗi app là một icon riêng trên điện thoại, mở toàn màn hình và dùng được khi không có mạng.

---

## Lưu trữ số liệu

Trong mỗi app, phần "Nguồn … định kỳ" có các nút quản lý dữ liệu.

**Mặc định — lưu trong app.** Chạy ngay, không cần làm gì. Điểm yếu: mất nếu bạn xoá dữ liệu duyệt web hoặc gỡ app, và không tự đồng bộ giữa các thiết bị.

**Tự lưu vào file trên máy (chỉ Chrome/Edge trên máy tính).** Bấm nút, chọn nơi lưu file `.json`. Từ đó mỗi lần bạn sửa gì, app tự ghi vào file đó. Lần sau mở app, nó tự đọc lại. Đặt file trong OneDrive hoặc Google Drive là có sao lưu tự động. Mỗi app dùng một file riêng, đừng trỏ hai app vào cùng một file. Sau khi khởi động lại máy, Windows có thể yêu cầu cấp lại quyền ghi. Lúc đó bạn bấm lại nút này một lần để kết nối lại.

Điện thoại và Firefox chưa hỗ trợ tính năng này, nên chỉ có cách lưu trong app kèm các nút chuyển dữ liệu bên dưới.

### Chuyển dữ liệu giữa laptop và điện thoại

Cách dễ nhất, không cần tài khoản hay máy chủ:

1. Trên máy có dữ liệu, bấm **Sao chép mã**. Mã dài khoảng 800 ký tự.
2. Gửi mã cho chính mình (Zalo, Messenger, Ghi chú...).
3. Trên máy kia, mở đúng app đó, bấm **Dán mã**, dán mã vào rồi bấm **Áp dụng**.

Mã dán bị xuống dòng hay thừa khoảng trắng vẫn nhập được. Mã bị cắt mất một đoạn sẽ bị từ chối và không làm hỏng dữ liệu hiện có. Dữ liệu trên máy nhận sẽ bị ghi đè, nên app hỏi xác nhận trước.

Nút **Chia sẻ file** (chỉ hiện trên máy hỗ trợ) gửi thẳng file dữ liệu qua bảng chia sẻ của điện thoại. **Xuất file / Nhập file** vẫn dùng được như cũ.

Muốn tự động đồng bộ không cần dán mã thì cần thêm một dịch vụ lưu trữ trên mạng (ví dụ Supabase hoặc Firebase). Bản này chưa có.

---

## Cập nhật app

Upload file mới đè lên repo. Lần mở tiếp theo, app hiện thông báo "Đã có bản cập nhật — Tải lại". Dữ liệu của bạn không bị mất.

## Thêm game thứ 6

Cả 5 app dùng chung một cấu trúc. Copy thư mục gần nhất về luật gacha rồi sửa trong `index.html`:

1. `<title>`, `.eyebrow`, `<h1>` — tên game và tên app
2. Tên tiền tệ (tìm và thay toàn bộ)
3. `id="g-single"` và `id="g-ten"` — giá 1 lượt và 10 lượt
4. `id="p-hard"` value — số lượt bảo hiểm
5. `HAS_5050` trong phần script — `true` nếu game có 50/50, `false` nếu không
6. Mảng `defaultSources` — danh sách nguồn thu mặc định
7. **`STORAGE_KEY`** — phải là tên khác hẳn các app còn lại, nếu không hai app sẽ ghi đè dữ liệu của nhau
8. Tên file trong `a.download = '...'` ở nút Xuất file

Rồi sửa `manifest.json` (name, short_name, theme_color), đổi icon, và sửa dòng `const CACHE` trong `sw.js` thành tên riêng. Cuối cùng thêm một thẻ `<a class="app">` vào `index.html` ở thư mục gốc.

## Gỡ app

Laptop: mở app, bấm menu ba chấm → Gỡ cài đặt. Android: giữ icon → Gỡ. iPhone: giữ icon → Xoá app. Dữ liệu trong file `.json` (nếu có dùng) không bị xoá.
