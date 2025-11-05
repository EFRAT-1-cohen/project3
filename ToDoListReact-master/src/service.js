import axios from "axios";

// הגדרת כתובת ה-API (כפי שהיה)
axios.defaults.baseURL = "http://localhost:5043";

// הוספת Interceptor לשגיאות (כפי שהיה)
axios.interceptors.response.use(
  (response) => {
    // מחזירים את התגובה כמו שהיא אם היא הצליחה
    return response;
  },
  (error) => {
    // רושמים שגיאות ללוג
    console.error("Axios Error Response:", error.message);
    if (error.response) {
      console.error("Status:", error.response.status);
      console.error("Data:", error.response.data);
    } else if (error.request) {
      console.error("No response received:", error.request);
    } else {
      console.error("Error setting up request:", error.message);
    }
    return Promise.reject(error);
  }
);

// --- מימוש פונקציות ---
// שימי לב שעכשיו אנחנו מחזירים את response.data

export const getTasks = async () => {
  const response = await axios.get("/api/items");
  return response.data; // <--- תיקון 1: מחזירים רק את המערך
};

export const addTask = async (taskName) => {
  // 1. בונים את האובייקט המלא שה-API מצפה לקבל
  const newTask = {
    name: taskName,
    isComplete: false
  };

  // 2. שולחים את האובייקט החדש (כ-JSON) ל-API
  const response = await axios.post("/api/items", newTask);
  return response.data;
};

export const updateTask = async (id, task) => {
  const response = await axios.put(`/api/items/${id}`, task);
  return response.status; // מחזירים 204 No Content
};

export const deleteTask = async (id) => {
  const response = await axios.delete(`/api/items/${id}`);
  return response.status; // מחזירים 204 No Content
};

// --- תיקון 2: הוספת הפונקציה החסרה ---
// App.js משתמש בפונקציה הזו כדי לעדכן רק סטטוס
// היא פשוט קוראת ל-updateTask עם הנתונים הנכונים
export const setCompleted = async (id, task, isComplete) => {
    // יוצרים עותק של המשימה ומעדכנים רק את השדה הרלוונטי
    const updatedTask = { ...task, isComplete: isComplete };
    // קוראים לפונקציית העדכון הכללית
    return updateTask(id, updatedTask);
};