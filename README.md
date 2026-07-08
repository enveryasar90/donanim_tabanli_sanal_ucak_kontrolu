# ✈️ Hardware-in-the-Loop (HIL) Hibrit Uçuş Simülatörü

Bu proje, fiziksel sensör verilerini ve standart USB oyun kumandası sinyallerini gerçek zamanlı olarak harmanlayarak Unity 6 oyun motorunda çalışan bir uçuş simülatörünü kontrol etmeyi amaçlayan bir donanım-yazılım entegrasyonu projesidir.

![Uçuş Demo](Images/ucak_surus.gif)

## 🚀 Proje Hakkında

Sistem, bir Arduino mikrodenetleyicisi ve MPU6050 jiroskop/ivmeölçer sensöründen alınan açısal verileri (Pitch ve Roll) asenkron seri haberleşme üzerinden Unity'ye aktarır. Yönlendirme fiziksel sensör ile yapılırken, gaz, fren ve motor sesi dinamikleri standart bir USB PS1 kolu (HID) üzerinden okunarak tam bir hibrit kokpit deneyimi sunulur.

## ✨ Temel Özellikler

* **Multi-Threading (Çoklu İş Parçacıklı) Mimari:** Seri port okuma işlemleri ana Unity döngüsünden (Main Thread) ayrılarak C# Thread'leri içine alınmıştır. Bu sayede kablo kopmaları veya sensör temassızlıklarında oyunun kilitlenmesi (DontDestroyOnLoad hataları) %100 engellenmiştir.
* **Gerçek Zamanlı Uçuş Fiziği:** Sensörden gelen ham veriler, uçağa ağırlık, atalet ve aerodinamik sürtünme hissi kazandıracak şekilde lineer interpolasyon (`Mathf.Lerp`) ile işlenmiştir.
* **Dinamik Ses Motoru:** Jet motorunun kükreme sesi (pitch), anlık uçuş hızına matematiksel olarak oranlanmış ve fiziksel gaz tuşuyla (R1) senkronize edilmiştir.
* **Hibrit Girdi Yönetimi:** Yönlendirme işlemleri I2C protokolüyle donanımdan, ateşleme ve hızlanma işlemleri ise PC'nin kendi Input kütüphanesinden eş zamanlı olarak çekilmektedir.

## 🛠️ Kullanılan Teknolojiler ve Donanımlar

**Yazılım:**
* Unity 6 (Fizik Motoru ve Simülasyon)
* C# (Oyun mantığı ve Thread yönetimi)
* Arduino IDE & C++ (Gömülü sistem kodlaması)

**Donanım:**
* Arduino UNO / Nano
* MPU6050 (6 Eksenli IMU Sensörü)
* PS1 USB Controller (Gaz ve fren kontrolleri için)
* Özel tasarım 3D baskı Gimbal / Mafsal standı

## 💻 Kurulum ve Çalıştırma

1. `Arduino` klasörü içindeki `.ino` kodunu Arduino kartınıza yükleyin.
2. Devreyi (VCC, GND, SDA, SCL) bağlayıp bilgisayarınızın USB portuna takın.
3. Unity projesini açın ve `Ucak_Merkez` objesine tıklayarak Inspector panelindeki `Port Adi` kısmına Arduino'nun bağlı olduğu COM portunu (Örn: COM4) yazın.
4. Oyunu başlatın (Play) ve uçuşun tadını çıkarın!

---
**Geliştirici:** Enver Yaşar
