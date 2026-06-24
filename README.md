# RentHouse API（後端）

RentHouse API 是一套租屋平台的後端系統，負責提供完整的 RESTful API，支援前端 Angular 應用進行資料存取與業務邏輯處理。

本專案採用 **ASP.NET Core Web API** 建構，並結合 **JWT 身分驗證、資料庫管理、圖片上傳、媒合系統與通知服務**，打造完整的租屋平台後端架構。

---

## 系統簡介

本系統為 RentHouse 平台的核心後端服務，負責處理以下核心業務：

- 使用者身份驗證與授權（JWT）
- 會員資料管理
- 房屋資料 CRUD
- 房屋媒合與推薦邏輯
- 圖片上傳與檔案管理
- 通知與系統訊息
- 與前端 Angular 進行 API 串接

---

## 技術架構

### 後端技術
- ASP.NET Core Web API
- C#
- Entity Framework Core
- SQL Server
- JWT Authentication
- RESTful API 架構

### 系統設計概念
- 前後端分離架構（Frontend / Backend Separation）
- 分層式架構（Controller / Service / Repository）
- Token-based Authentication（JWT）
- 模組化功能設計
- 可擴充 API 架構

---

## 系統架構

```
Frontend (Angular 21)
        ↓
REST API (ASP.NET Core)
        ↓
Service Layer
        ↓
Repository Layer
        ↓
SQL Server Database
```

---

## 主要功能模組

### 1. 身分驗證系統
- 使用者註冊 / 登入
- JWT Token 驗證
- Role-based Authorization（會員 / 房東 / 管理者）
- Token 驗證 Middleware

---

### 2. 會員系統
- 使用者資料管理
- 個人資訊更新
- 頭像上傳
- 使用者狀態管理

---

### 3. 房屋管理系統
- 房屋新增 / 編輯 / 刪除
- 房屋列表查詢
- 房屋詳細資訊
- 房屋分類（如坪數、價格、地區）

---

### 4. 房屋媒合系統
- 使用條件進行房源媒合
- 簡易推薦邏輯（依需求條件篩選）
- 使用者偏好匹配（可擴充 AI）

---

### 5. 圖片與檔案系統
- 房屋圖片上傳
- 使用者頭像上傳
- 檔案儲存與路徑管理

---

### 6. 通知系統
- 系統通知（公告 / 媒合結果）
- 使用者訊息提示
- 可擴充即時通知（SignalR 可延伸）

---

## API 架構設計

### Auth
```
POST   /api/auth/login
POST   /api/auth/register
GET    /api/auth/profile
```

### Users
```
GET    /api/users
GET    /api/users/{id}
PUT    /api/users/{id}
```

### Houses
```
GET    /api/houses
GET    /api/houses/{id}
POST   /api/houses
PUT    /api/houses/{id}
DELETE /api/houses/{id}
```

### Match
```
POST   /api/match/search
POST   /api/match/recommend
```

### Upload
```
POST   /api/upload/image
POST   /api/upload/avatar
```

---

## 資料庫設計概念

主要資料表包含：

- Users（使用者）
- Houses（房屋）
- HouseImages（房屋圖片）
- Matches（媒合紀錄）
- Notifications（通知）
- Roles（權限）

---

## 開發環境

啟動專案：

```bash
dotnet run
```

或使用 Visual Studio：

```
IIS Express / Kestrel
```

---

## API 測試工具

建議使用：

- Postman
- Swagger UI（開發環境）

Swagger 預設：

```
https://localhost:{port}/swagger
```

---

## 安全機制

- JWT Token 驗證
- Password Hash（不可明碼儲存）
- Role-based Authorization
- CORS 控制
- API 權限驗證 Middleware

---

## 專案特色

- 完整前後端分離架構
- RESTful API 標準設計
- 可擴充媒合系統
- 模組化 Service 架構
- 支援圖片與檔案管理
- JWT 安全機制
- 可延伸即時通知（SignalR）

---

## 未來優化方向

- [ ] 導入 SignalR（即時聊天 / 通知）
- [ ] AI 房屋媒合推薦系統
- [ ] Redis 快取優化查詢
- [ ] ElasticSearch 房源搜尋優化
- [ ] 行為分析與推薦系統
- [ ] Docker 化部署
- [ ] CI/CD 自動部署

---

## 開發者說明

RentHouse Backend API  
Powered by ASP.NET Core 🚀

本專案為 RentHouse 租屋平台核心後端服務，負責所有商業邏輯與資料處理。
