# Shadow Switch (Godot 4 + C#)

`Shadow Switch` la prototype game 2D puzzle-platformer, nơi người chơi điều khiển **2 nhân vật song song** trong hai thế giới:

- 🌞 **Ánh sáng**
- 🌑 **Bóng tối**

Hai thế giới có bố cục tương tự nhưng có khác biệt về nền tảng, công tắc và đường đi.

## Core Gameplay

- Di chuyển trái/phải, nhảy, leo thang.
- Nhấn `Space` để chuyển trạng thái giữa thế giới Ánh sáng và Bóng tối.
- Ở thế giới Bóng tối có **giới hạn thời gian**.
- Cơ chế liên kết puzzle:
  - Nút ở Ánh sáng kích hoạt cầu ở Bóng tối.
  - Công tắc ở Bóng tối mở/đóng cổng.
- Mục tiêu: đưa **cả 2 nhân vật** đến vùng đích tương ứng.

## Controls

- `A / D` hoặc `← / →`: Di chuyển
- `W` hoặc `Enter`: Nhảy
- `W / S` hoặc `↑ / ↓`: Leo thang
- `Space`: Switch Ánh sáng ↔ Bóng tối
- `E`: Tương tác công tắc
- `R`: Chơi lại khi hết thời gian ở Bóng tối

## Technical Notes

- Dự án dùng Godot 4 (.NET) và C#.
- Scene chính là `Main.tscn`.
- Toàn bộ prototype gameplay được dựng bằng code trong `Scripts/Main.cs`:
  - Tạo world frame, platform, trigger, ladder, goal.
  - Điều khiển movement cho 2 `CharacterBody2D`.
  - Puzzle logic world-link.
  - Dark timer + trạng thái thắng/thua.

## Run

1. Mở project bằng **Godot 4.x (.NET)**.
2. Run scene `Main.tscn`.
3. Hoàn thành puzzle bằng cách phối hợp hai thế giới.

## Gợi ý mở rộng

- Thêm quái chỉ tồn tại ở Bóng tối.
- Mỗi world có kỹ năng riêng.
- Hệ thống tính điểm theo thời gian.
- Local co-op: 2 người điều khiển 2 nhân vật.
