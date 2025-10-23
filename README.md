# Book Projects

Demo: https://youtu.be/rK64OZJCxcU

Hệ thống lưu trữ và đọc tài liệu online.  

## 🚀 Tính năng chính

1. Quản lý Tài liệu (Document Management)

CRUD đầy đủ: Tạo, Xem, Sửa, Xóa tài liệu
Thông tin lưu trữ: Tiêu đề, Lĩnh vực, Tác giả, Mô tả, Thời gian tạo/sửa
Hỗ trợ đa dạng định dạng:

.pdf - Tự động chia thành từng trang
.xlsx - Chuyển mỗi sheet thành trang HTML
.txt - Chia theo 700 từ/trang
.docx - Lưu trữ file gốc


Upload file: Với progress bar, validation kích thước (max 50MB)

2. Phân trang Tài liệu (Document Pages)

Tự động chia tài liệu thành nhiều trang
Xem theo 2 chế độ:

Original Mode: Xem file gốc
Paged Mode: Xem từng trang với điều hướng


Chỉnh sửa nội dung từng trang
Điều hướng trang (Previous, Next, Last page)

3. Bookmark

Đánh dấu trang đang đọc
Mỗi trang chỉ có 1 bookmark (unique constraint)
Quản lý danh sách bookmark
Tìm kiếm bookmark theo tên tài liệu
Truy cập nhanh đến trang đã bookmark
Validation: Không cho phép tạo bookmark trùng

4. Tìm kiếm & Lọc

Tìm kiếm tài liệu theo: Tiêu đề, Tác giả, Lĩnh vực
Tìm kiếm bookmark theo tên tài liệu
AJAX search với debounce (500ms)
Phân trang kết quả (20 items/page)

5. SignalR Real-time Updates 
Đây là điểm nổi bật của project:
Notifications toàn cục:

Thông báo khi có tài liệu mới
Thông báo khi tài liệu bị xóa/sửa
Thông báo khi có bookmark mới/xóa

Auto-refresh:

Danh sách tài liệu tự động reload khi có thay đổi
Danh sách bookmark tự động reload
Trang tài liệu tự động reload khi nội dung thay đổi

Document Group Notifications:

Người đang xem cùng tài liệu nhận thông báo real-time
Thông báo khi trang được chỉnh sửa
Collaboration awareness (ai đang edit tài liệu)

Notification Panel:

Hiển thị lịch sử thông báo
Đánh dấu đã đọc/chưa đọc
Connection status indicator
Toast notifications với animations


Code coverage cho các services chính

