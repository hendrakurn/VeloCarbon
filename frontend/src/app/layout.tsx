import './globals.css';
import Sidebar from '@/components/Sidebar';

export const metadata = {
  title: 'VeloCarbon — Desktop Prototype',
  description: 'Aplikasi desktop pelacakan jejak karbon pribadi dan kos berbasis MVVM C#',
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="id">
      <body className="flex bg-slate-100 min-h-screen">
        <Sidebar />
        <main className="flex-1 flex flex-col min-w-0 overflow-y-auto">
          {children}
        </main>
      </body>
    </html>
  );
}
