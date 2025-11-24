/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './Pages/**/*.cshtml',
    './Views/**/*.cshtml',
    './Areas/**/*.cshtml'
  ],
  theme: {
    extend: {
      colors: {
        'primary': '#0ea5e9',
        'secondary': '#10b981',
        'danger': '#ef4444',
        'warning': '#f59e0b',
      }
    },
  },
  plugins: [],
}
