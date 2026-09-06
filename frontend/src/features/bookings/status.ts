export const appointmentStatusLabels: Record<string, string> = {
  AwaitingDeposit: "Chờ đặt cọc",
  Pending: "Chờ nhân viên xác nhận lịch đổi",
  Confirmed: "Đã xác nhận",
  InProgress: "Đang thực hiện",
  Completed: "Đã hoàn thành",
  Cancelled: "Đã hủy",
};

export const appointmentStatusClasses: Record<string, string> = {
  AwaitingDeposit: "bg-amber-100 text-amber-800",
  Pending: "bg-orange-100 text-orange-800",
  Confirmed: "bg-emerald-100 text-emerald-800",
  InProgress: "bg-sky-100 text-sky-800",
  Completed: "bg-blue-100 text-blue-800",
  Cancelled: "bg-slate-200 text-slate-700",
};

export const depositStatusLabels: Record<string, string> = {
  AwaitingReceipt: "Chờ thanh toán qua payOS",
  ReceiptSubmitted: "Đang kiểm tra giao dịch cũ",
  Approved: "Đã thanh toán",
  Rejected: "Đã hủy",
  Expired: "Đã hết hạn",
};
