import { Routes, Route } from "react-router-dom";
import AuthBootstrap from "@/components/AuthBootstrap";
import HomePage from "@/pages/customer/HomePage";
import NailDesignsPage from "@/pages/customer/NailDesignsPage";
import LoginPage from "@/pages/auth/LoginPage";
import RegisterPage from "@/pages/auth/RegisterPage";
import ForgotPasswordPage from "@/pages/auth/ForgotPasswordPage";
import GoogleCallbackPage from "@/pages/auth/GoogleCallbackPage";
import ServicesPage from "@/pages/ServicesPage";
import BookingPage from "@/pages/BookingPage";
import AppointmentsPage from "@/pages/customer/AppointmentsPage";
import NotificationCenter from "@/components/NotificationCenter";
import StaffAppointmentsPage from "@/pages/staff/StaffAppointmentsPage";
import DepositPage from "@/pages/DepositPage";
import ScrollToTop from "@/components/ScrollToTop";

export default function App() {
  return (
    <AuthBootstrap>
      <ScrollToTop />
      <Routes>
        <Route path="/" element={<HomePage />} />
        <Route path="/nail-designs" element={<NailDesignsPage />} />
        <Route path="/services" element={<ServicesPage />} />
        <Route path="/booking" element={<BookingPage />} />
        <Route path="/deposit/:appointmentId" element={<DepositPage />} />
        <Route path="/appointments" element={<AppointmentsPage />} />
        <Route path="/staff/appointments" element={<StaffAppointmentsPage />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route path="/forgot-password" element={<ForgotPasswordPage />} />
        <Route path="/auth/google/callback" element={<GoogleCallbackPage />} />
      </Routes>
      <NotificationCenter />
    </AuthBootstrap>
  );
}
