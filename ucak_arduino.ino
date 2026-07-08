#include <Wire.h>

// MPU6050 I2C adresi (Genelde 0x68'dir)
const int MPU_addr = 0x68; 
int16_t AcX, AcY, AcZ;

void setup() {
  Serial.begin(115200); // Unity'deki kodumuzla birebir aynı hız
  
  Wire.begin();
  Wire.beginTransmission(MPU_addr);
  Wire.write(0x6B); // Sensörü uyandırma register'ı
  Wire.write(0);    // Sensörü uyandırıyoruz
  Wire.endTransmission(true);
}

void loop() {
  Wire.beginTransmission(MPU_addr);
  Wire.write(0x3B); // İvmeölçer verilerinin başladığı adres
  Wire.endTransmission(false);
  Wire.requestFrom(MPU_addr, 6, true); // 6 byte veri istiyoruz (X, Y, Z için ikişer byte)
  
  // Ham ivmeölçer verilerini okuyoruz
  AcX = Wire.read() << 8 | Wire.read();
  AcY = Wire.read() << 8 | Wire.read();
  AcZ = Wire.read() << 8 | Wire.read();
  
  // Unity kodunun kilitlenmeden okuyabileceği Pitch ve Roll açı hesaplamaları (atan2)
  float roll = atan2(AcY, AcZ) * 180.0 / M_PI;
  float pitch = atan2(-AcX, sqrt((float)AcY * AcY + (float)AcZ * AcZ)) * 180.0 / M_PI;
  
  // Unity'nin beklediği format: "pitch,roll" (Örn: 12.45,-5.20)
  Serial.print(pitch);
  Serial.print(",");
  Serial.println(roll);
  
  delay(15); // Unity'nin ReadTimeout (50ms) süresine yakalanmamak için ideal kararlılık süresi
}