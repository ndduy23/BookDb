# 🔖 Bookmark Notification System - Documentation

## Tổng quan
Hệ thống thông báo bookmark với SignalR và AJAX, cung cấp feedback rõ ràng khi lưu/xóa bookmark.

## ✨ Tính năng chính

### 1. **Thông báo khi lưu Bookmark**
Khi người dùng nhấn nút "Thêm Bookmark" trên trang xem tài liệu:

#### ✅ **Lưu thành công:**
- Hiển thị toast notification: `✅ Bookmark đã được lưu thành công!`
- Nút chuyển sang màu vàng với text: `✓ Đã có Bookmark`
- Animation pulse effect trên nút
- Nút bị disabled để tránh tạo duplicate
- Tự động reload trang sau 2 giây
- Gửi thông báo SignalR đến tất cả users

#### ❌ **Lưu thất bại (đã có bookmark):**
- Hiển thị toast notification: `⚠️ Không lưu được vì đã có bookmark trên trang này`
- Nút chuyển sang màu vàng với text: `✓ Đã có Bookmark`
- Nút bị disabled
- Không reload trang

#### ⚠️ **Lỗi khác:**
- Hiển thị toast notification với icon `❌` và message lỗi cụ thể
- Nút vẫn enabled để user thử lại
- Log error để debug

### 2. **Visual Feedback**

#### Loading State:
```
[Spinner] Thêm Bookmark (nút bị disabled)
```

#### Success State:
```
✓ Đã có Bookmark (nút màu vàng, disabled)
```

#### Animation:
- Pulse animation khi lưu thành công
- Smooth transition giữa các states
- Toast notifications slide in từ bên phải

### 3. **Integration với SignalR**

#### Real-time notifications:
- Khi User A tạo bookmark → User B nhận notification
- Khi User A xóa bookmark → User B nhận notification
- Tự động cập nhật danh sách bookmark trên tất cả tabs

## 📁 Files đã thay đổi

### Backend:

1. **`BookmarksController.cs`**
   - Thêm hỗ trợ AJAX requests
   - Trả về JSON response với `success`, `message`, `bookmarkId`
   - Kiểm tra `X-Requested-With` header

2. **`BookmarkService.cs`**
   - Cập nhật return type: `(bool Success, string? ErrorMessage, int? BookmarkId)`
   - Message rõ ràng: "Không lưu được vì đã có bookmark trên trang này"
   - Trả về BookmarkId khi tạo thành công

3. **`IBookmarkService.cs`**
   - Cập nhật interface cho CreateBookmarkAsync

### Frontend:

1. **`ViewDocument.cshtml`**
   - Chuyển form submit sang AJAX
   - Hiển thị toast notifications thay vì alerts
   - Loading spinner trên nút
   - CSS animations cho button states
   - Xử lý TempData messages

## 🎯 User Flow

```
User clicks "Thêm Bookmark"
    ↓
Button shows loading spinner
    ↓
AJAX POST to /bookmarks/create
    ↓
Controller checks if bookmark exists
    ↓
┌─────────────────┬─────────────────┐
│   Exists        │   Not exists    │
│   (Error)       │   (Success)     │
└─────────────────┴─────────────────┘
    ↓                   ↓
Warning toast      Success toast
Button → Yellow    Button → Yellow
Disabled           Disabled
                       ↓
                SignalR notification
                       ↓
                Reload page (2s)
```

## 🔧 API Response Format

### Success Response:
```json
{
  "success": true,
  "message": "Bookmark đã được lưu thành công!",
  "bookmarkId": 123
}
```

### Error Response (Already Exists):
```json
{
  "success": false,
  "message": "Không lưu được vì đã có bookmark trên trang này."
}
```

### Error Response (Other):
```json
{
  "success": false,
  "message": "Trang tài liệu không tồn tại."
}
```

## 🎨 Toast Notification Types

| Type    | Icon | Color | Usage |
|---------|------|-------|-------|
| success | ✅   | Green | Bookmark saved successfully |
| warning | ⚠️   | Yellow | Bookmark already exists |
| error   | ❌   | Red   | Server error or validation error |

## 📱 Responsive Design

- Toast notifications tự động điều chỉnh theo màn hình
- Nút bookmark có min-width để tránh resize khi thay đổi text
- Animations mượt mà trên tất cả devices

## 🔍 Testing

### Manual Testing:

1. **Test lưu bookmark lần đầu:**
   - Vào trang xem tài liệu (mode paged)
   - Click "Thêm Bookmark"
   - Kiểm tra toast notification xuất hiện
   - Kiểm tra nút chuyển sang "Đã có Bookmark"

2. **Test lưu bookmark khi đã tồn tại:**
   - Reload trang có bookmark
   - Nút hiện "Đã có Bookmark" ngay từ đầu
   - Click nút → hiển thị warning toast

3. **Test multi-tab:**
   - Mở 2 tabs cùng page
   - Tạo bookmark ở tab 1
   - Tab 2 nhận notification qua SignalR

### Automated Testing:
```csharp
// Test in BookmarksControllerTests.cs
[Fact]
public async Task Create_Bookmark_Returns_Success_Json()
{
    // Test AJAX request returns JSON
}

[Fact]
public async Task Create_Bookmark_Already_Exists_Returns_Error()
{
    // Test duplicate bookmark error message
}
```

## 🐛 Troubleshooting

### Issue: Toast không hiển thị
**Solution:** Kiểm tra `window.NotificationHub` đã load chưa

### Issue: Nút không đổi màu
**Solution:** Kiểm tra response.success và message trong console

### Issue: SignalR không gửi notification
**Solution:** Kiểm tra connection status trong browser console

## 📝 Code Examples

### JavaScript - Handle bookmark save:
```javascript
$('#bookmarkForm').on('submit', function(e) {
    e.preventDefault();
    
    $.ajax({
        url: form.attr('action'),
        type: 'POST',
        data: form.serialize(),
        headers: {
            'X-Requested-With': 'XMLHttpRequest'
        },
        success: function(response) {
            if (response.success) {
                window.NotificationHub.showLocal('✅ ' + response.message, 'success');
            } else {
                window.NotificationHub.showLocal('⚠️ ' + response.message, 'warning');
            }
        }
    });
});
```

### C# - Controller action:
```csharp
[HttpPost("create")]
public async Task<IActionResult> Create(int documentPageId, string? title)
{
    var result = await _bookmarkService.CreateBookmarkAsync(documentPageId, title, url);
    
    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
    {
        return Json(new { 
            success = result.Success, 
            message = result.ErrorMessage ?? "Bookmark đã được lưu thành công!",
            bookmarkId = result.BookmarkId ?? 0
        });
    }
    
    return Redirect(Request.Headers["Referer"].ToString() ?? "/");
}
```

## 🚀 Future Enhancements

- [ ] Thêm tùy chọn edit bookmark title
- [ ] Bookmark notes/description
- [ ] Bookmark categories/tags
- [ ] Export bookmarks
- [ ] Share bookmarks với users khác
- [ ] Undo delete bookmark

## 📚 Related Files

- `/workspace/BookDb/Controllers/BookmarksController.cs`
- `/workspace/BookDb/Services/Implementations/BookmarkService.cs`
- `/workspace/BookDb/Services/Interfaces/IBookmarkService.cs`
- `/workspace/BookDb/Views/Documents/ViewDocument.cshtml`
- `/workspace/BookDb/wwwroot/js/global-notifications.js`
- `/workspace/BookDb/wwwroot/js/bookmark-signalr.js`
