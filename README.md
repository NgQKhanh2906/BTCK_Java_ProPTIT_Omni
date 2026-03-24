# Bài tập cuối khóa Java Core - Game Omni

Dự án xây dựng trò chơi 2D sử dụng ngôn ngữ C# và nền tảng Unity.
Đây là sản phẩm của nhóm 4 thành viên:
- Nguyễn Quốc Khánh
- Nguyễn Tiến Thịnh
- Bùi Đức Tuân
- Phạm Hữu Chiến

## Giới thiệu dự án
* **Thể loại:** Phiêu lưu, nhập vai, giải đố
* **Cốt truyện:** [text](https://docs.google.com/document/d/1ObKSVhkorQwts1xlLoshfNkJTmZhT_a_ivdUwThum0Y/edit?tab=t.0)
* **Công cụ phát triển:**
    * **Engine**: Unity 2022.3.62f2
    * **IDE:** JetBrains Rider hoặc Visual Studio
    * **Ngôn ngữ**: C#
    * **UI Library:** Unity UI, 
    * **Version Control:** Git & GitHub

## Hướng dẫn cài đặt & Chạy Game
1.  **Yêu cầu:** Máy đã cài JDK 25 và cài đặt thư viện JavaFX.
2.  **Clone project:**
    ```bash
    git clone https://github.com/NgQKhanh2906/BTCK_Java_ProPTIT_Omni.git
    ```
3. **Mở bằng Unity:**
    * Mở Unity Hub -> `Add` -> Chọn thư mục project đã clone.
4. **Chạy ứng dụng:**
    * Mở Scene `MainMusic` hoặc `Menu`.
    * Nhấn nút **Play** ở giữa màn hình Editor.

## Quy trình làm việc
Để tránh xung đột file `.unity` (Scene) và `.prefab` (rất khó merge):
* **Nguyên tắc:** Mỗi thành viên làm việc trên một Scene riêng hoặc Prefab riêng.
* `main`: Bản build ổn định nhất.
* `develop`: Nhánh trộn code và tài nguyên.
* **Workflow:** `Pull` -> `Tạo nhánh tính năng` -> `Commit` -> `Push` -> `Pull Request`.

## Drive cập nhật tài nguyên của dự án
[text](https://drive.google.com/drive/folders/1Y1zN5itLUCxADhx9lBpiApcCNkRddtyM)

---
*Dự án đang trong quá trình phát triển.*