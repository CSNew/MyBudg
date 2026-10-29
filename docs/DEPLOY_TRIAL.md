# ทดลอง deploy บนแผนฟรี (Render + Neon)

> **ใช้ข้อมูลสมมติเท่านั้น** แผนฟรีไม่มี backup ที่รับประกัน และเงื่อนไขของผู้ให้บริการเปลี่ยนได้
> เช็กหน้าเว็บทางการของ Render/Neon ก่อนสมัคร (ข้อมูลในเอกสารนี้ตรวจล่าสุดจากหน้าเว็บทางการเมื่อวางแผน)

## ภาพรวม

```
ผู้ใช้ ──HTTPS──▶ Render (Docker, free, Singapore) ──SSL──▶ Neon Postgres (free)
                   │  /healthz
                   └─ ค่าลับอยู่ใน Environment ของ Render
```

ข้อจำกัดของแผนฟรี: บริการ Render หลับเมื่อไม่มีผู้ใช้ 15 นาที (ปลุกใหม่ราว 1 นาที), ไม่มีดิสก์ถาวร
(เราไม่ต้องใช้ เพราะข้อมูลและ key ring อยู่ใน Postgres), Neon ฟรีมีพื้นที่ 0.5 GB

## ขั้นตอน

1. **Neon**: สร้างโปรเจกต์ → คัดลอก connection string (รูปแบบ `postgres://user:pass@host/db?sslmode=require`)
2. **สร้างค่าลับ** (เก็บสำรองไว้นอก Render):
   ```bash
   openssl rand -base64 32      # → Encryption__Keys__k1
   openssl rand -base64 18      # → Registration__InviteCode (รหัสเชิญ)
   ```
3. **Render**: New → Blueprint → เลือก repo นี้ (ใช้ `render.yaml`) แล้วกรอกค่าที่ `sync: false`:
   `DATABASE_URL` (จากข้อ 1), `Encryption__Keys__k1`, `Registration__InviteCode`
4. รอ build เสร็จ แล้วเปิด URL → สมัครด้วยรหัสเชิญ

> `render.yaml` เขียนจากความรู้ของรูปแบบ Blueprint ทั่วไป ยังไม่ได้ทดสอบกับ Render จริง
> ถ้าฟิลด์ใดไม่ตรง ให้ตั้งค่าเทียบเท่าในหน้าเว็บ (Runtime: Docker, Health Check Path: `/healthz`, Region: Singapore)

## ตัวแปรสภาพแวดล้อมที่ใช้

| ตัวแปร | ความหมาย |
|---|---|
| `Database__Provider` | `Sqlite` (ค่าเริ่มต้น) หรือ `Postgres` |
| `DATABASE_URL` / `ConnectionStrings__Postgres` | connection string ของ Postgres (URL หรือ key=value; บังคับ SSL เสมอ) |
| `Database__MigrateOnStartup` | `true` (ค่าเริ่มต้น) — ถ้ารันหลาย instance ให้ปิดแล้วรัน migration จาก pipeline |
| `ForwardedHeaders__Enabled` | `true` เมื่อมี reverse proxy หน้าแอปเป็นทางเข้าเดียว (Render) |
| `Registration__Mode` / `Registration__InviteCode` | ดู `docs/SECURITY.md` |
| `Encryption__CurrentKeyId`, `Encryption__Keys__<id>` | คีย์เข้ารหัสข้อมูล (บังคับใน Production) |
| `PORT` | พอร์ตที่ PaaS กำหนด (แอปรับอัตโนมัติ) |

## รันด้วย Docker ในเครื่อง (ทดสอบก่อนขึ้นคลาวด์)

```bash
docker build -t mybudg .
docker run --rm -p 8080:8080 \
  -e Database__Provider=Postgres \
  -e DATABASE_URL='postgres://user:pass@host:5432/db' \
  -e Encryption__CurrentKeyId=k1 -e Encryption__Keys__k1="$(openssl rand -base64 32)" \
  -e Registration__Mode=Open \
  mybudg
```

## ตรวจหลัง deploy

- `GET /healthz` ตอบ `ok`
- สมัคร → จดรายการ → รอให้บริการหลับแล้วเปิดใหม่ → **ยังล็อกอินอยู่และข้อมูลยังอยู่** (พิสูจน์ว่า key ring อยู่ในฐานข้อมูล)
- ใน Neon SQL editor: `select "Note","Amount" from "Transactions" limit 3;` ต้องเห็นเป็น `v1:...`
- ลองใส่รหัสผ่านผิดหลายครั้ง → ถูกล็อกเฉพาะบัญชีนั้น ไม่กระทบผู้ใช้อื่น

## ก่อนใช้ข้อมูลจริง

ต้องทำแผนงานความปลอดภัยต่อ (2FA, อีเมลยืนยัน, envelope encryption + Key Vault, audit log, PDPA) และย้ายไปบริการที่มี backup ชัดเจน
