# Publikasi Pembaruan PTSP Assistant

## Aturan utama

Jangan menaikkan `stable/latest.json` sebelum asset installer sudah tersedia dan dapat diunduh dari GitHub Release. Extension akan mempercayai versi, ukuran, dan SHA-256 yang tercantum dalam manifest stable.

## Publikasi v3.0.4

Buat GitHub Release dengan tag:

```text
v3.0.4
```

Unggah asset berikut tanpa mengubah namanya:

```text
PTSP-Assistant-Setup-v3.0.4-FULL-SHARING.exe
```

Verifikasi sebelum publish:

```text
Ukuran    : 31.037.952 byte
SHA-256   : 59efbe922f17b68d2da5a2af902b3dbcbb74c834160cc000d78a1a52a38b2b89
```

Setelah release diterbitkan, pastikan URL berikut mengunduh file yang benar:

```text
https://github.com/xianjieng-learn/ptsp-assistant-releases/releases/download/v3.0.4/PTSP-Assistant-Setup-v3.0.4-FULL-SHARING.exe
```

## Publikasi versi berikutnya

1. Bangun installer FULL versi baru.
2. Hitung ukuran file dan SHA-256.
3. Buat catatan perubahan di `releases/vX.Y.Z.md`.
4. Buat GitHub Release bertag `vX.Y.Z` dan unggah installer.
5. Pastikan asset dapat diunduh melalui HTTPS.
6. Baru perbarui `stable/latest.json` sebagai commit terakhir.
7. Uji dari satu komputer channel stable sebelum digunakan di semua komputer.

## Melalui workflow

Workflow **Publish PTSP Assistant Release** dapat dijalankan dari tab Actions. Masukkan URL HTTPS langsung ke installer pada input `asset_url`. Workflow akan:

- membaca nama, ukuran, dan SHA-256 dari `stable/latest.json`;
- mengunduh installer;
- memastikan file merupakan PE32+ Windows;
- memverifikasi ukuran dan SHA-256;
- membuat atau memperbarui GitHub Release;
- menguji URL asset publik.

## Rollback manifest

Apabila release bermasalah, kembalikan `stable/latest.json` ke versi stable sebelumnya. Jangan mengganti isi asset pada tag lama dengan build berbeda tanpa memperbarui SHA-256 dan melakukan pengujian ulang.
