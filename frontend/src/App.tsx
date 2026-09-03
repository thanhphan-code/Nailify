import { Routes, Route } from "react-router-dom";
import AuthBootstrap from "@/components/AuthBootstrap";
import HomePage from "@/pages/customer/HomePage";
import NailDesignsPage from "@/pages/customer/NailDesignsPage";
import LoginPage from "@/pages/auth/LoginPage";
import RegisterPage from "@/pages/auth/RegisterPage";
import ForgotPasswordPage from "@/pages/auth/ForgotPasswordPage";
import GoogleCallbackPage from "@/pages/auth/GoogleCallbackPage";
import ServicesPage from "@/pages/ServicesPage";

export default function App() {
  return (
    <AuthBootstrap>
      <Routes>
        <Route path="/" element={<HomePage />} />
        <Route path="/nail-designs" element={<NailDesignsPage />} />
        <Route path="/services" element={<ServicesPage />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route path="/forgot-password" element={<ForgotPasswordPage />} />
        <Route path="/auth/google/callback" element={<GoogleCallbackPage />} />
      </Routes>
    </AuthBootstrap>
  );
}
