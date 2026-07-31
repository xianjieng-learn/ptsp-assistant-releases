# PTSP Assistant Releases

Repository publik untuk distribusi pembaruan **PTSP Assistant**.

## Struktur

- `stable/latest.json` — manifest versi stable yang dibaca extension dan PTSP Update Agent.
- `stable/schema.json` — dokumentasi format manifest pembaruan.
- `releases/vX.Y.Z.md` — catatan perubahan setiap versi.
- `.github/workflows/publish-release.yml` — publikasi asset dengan pemeriksaan ukuran dan SHA-256.
- `PUBLISHING.md` — prosedur publikasi dan rollback pembaruan.

## Alur pembaruan

1. Extension memeriksa `stable/latest.json` saat Side Panel dibuka dan setiap 12 jam.
2. Pengguna menekan **Update Sekarang**.
3. PTSP Update Agent mengunduh installer dari GitHub Release.
4. Agent memverifikasi host sumber, ukuran file, dan SHA-256.
5. Installer memperbarui extension dan komponen Windows dalam satu alur serta mempertahankan data pengguna.
6. Extension menampilkan tombol **Muat Ulang** setelah pemasangan selesai.

## Keamanan

Installer hanya diterima dari repository `xianjieng-learn/ptsp-assistant-releases` melalui HTTPS dan wajib cocok dengan ukuran serta SHA-256 pada manifest stable.

## Rilis stable saat ini

Lihat [`stable/latest.json`](stable/latest.json).

Prosedur penerbitan tersedia di [`PUBLISHING.md`](PUBLISHING.md).
