// State Manager - Quản lý state của ứng dụng
class StateManager {
  constructor() {
    this.storageKey = "classManagementState";
    this.init();
  }

  init() {
    // Khôi phục state khi trang load
    this.restoreState();

    // Lưu state khi trang sắp đóng
    window.addEventListener("beforeunload", () => {
      this.saveState();
    });

    // Lưu state khi có thay đổi
    this.setupAutoSave();
  }

  // Lưu state vào localStorage
  saveState() {
    const state = {
      currentPage: window.location.pathname,
      timestamp: new Date().toISOString(),
      userPreferences: this.getUserPreferences(),
      formData: this.getFormData(),
      uiState: this.getUIState(),
    };

    try {
      localStorage.setItem(this.storageKey, JSON.stringify(state));
      console.log("State saved successfully");
    } catch (error) {
      console.error("Error saving state:", error);
    }
  }

  // Khôi phục state từ localStorage
  restoreState() {
    try {
      const savedState = localStorage.getItem(this.storageKey);
      if (savedState) {
        const state = JSON.parse(savedState);

        // Kiểm tra xem state có cũ không (quá 24 giờ)
        const stateTime = new Date(state.timestamp);
        const now = new Date();
        const hoursDiff = (now - stateTime) / (1000 * 60 * 60);

        if (hoursDiff < 24) {
          this.applyState(state);
          console.log("State restored successfully");
        } else {
          // Xóa state cũ
          localStorage.removeItem(this.storageKey);
        }
      }
    } catch (error) {
      console.error("Error restoring state:", error);
    }
  }

  // Áp dụng state đã lưu
  applyState(state) {
    // Khôi phục user preferences
    if (state.userPreferences) {
      this.applyUserPreferences(state.userPreferences);
    }

    // Khôi phục form data
    if (state.formData) {
      this.applyFormData(state.formData);
    }

    // Khôi phục UI state
    if (state.uiState) {
      this.applyUIState(state.uiState);
    }
  }

  // Lấy user preferences
  getUserPreferences() {
    return {
      theme: document.documentElement.getAttribute("data-theme") || "light",
      sidebarCollapsed:
        document.querySelector(".sidebar")?.classList.contains("collapsed") ||
        false,
      language: document.documentElement.lang || "vi",
    };
  }

  // Áp dụng user preferences
  applyUserPreferences(preferences) {
    if (preferences.theme) {
      document.documentElement.setAttribute("data-theme", preferences.theme);
    }

    if (preferences.sidebarCollapsed) {
      const sidebar = document.querySelector(".sidebar");
      if (sidebar) {
        sidebar.classList.add("collapsed");
      }
    }
  }

  // Lấy form data
  getFormData() {
    const forms = document.querySelectorAll("form");
    const formData = {};

    forms.forEach((form, index) => {
      const formId = form.id || `form_${index}`;
      const inputs = form.querySelectorAll("input, select, textarea");
      const data = {};

      inputs.forEach((input) => {
        if (input.type === "checkbox" || input.type === "radio") {
          data[input.name] = input.checked;
        } else {
          data[input.name] = input.value;
        }
      });

      if (Object.keys(data).length > 0) {
        formData[formId] = data;
      }
    });

    return formData;
  }

  // Áp dụng form data
  applyFormData(formData) {
    Object.keys(formData).forEach((formId) => {
      const form =
        document.getElementById(formId) ||
        document.querySelector(`form:nth-child(${formId.split("_")[1]})`);
      if (form) {
        const data = formData[formId];
        Object.keys(data).forEach((name) => {
          const input = form.querySelector(`[name="${name}"]`);
          if (input) {
            if (input.type === "checkbox" || input.type === "radio") {
              input.checked = data[name];
            } else {
              input.value = data[name];
            }
          }
        });
      }
    });
  }

  // Lấy UI state
  getUIState() {
    return {
      activeTab: document.querySelector(".nav-link.active")?.textContent,
      scrollPosition: window.scrollY,
      modalOpen: document.querySelector(".modal.show") !== null,
      expandedSections: this.getExpandedSections(),
    };
  }

  // Áp dụng UI state
  applyUIState(uiState) {
    if (uiState.scrollPosition) {
      window.scrollTo(0, uiState.scrollPosition);
    }

    if (uiState.expandedSections) {
      this.applyExpandedSections(uiState.expandedSections);
    }
  }

  // Lấy các section đã expand
  getExpandedSections() {
    const expanded = {};
    document.querySelectorAll(".collapse").forEach((collapse) => {
      if (collapse.classList.contains("show")) {
        expanded[collapse.id] = true;
      }
    });
    return expanded;
  }

  // Áp dụng các section đã expand
  applyExpandedSections(expandedSections) {
    Object.keys(expandedSections).forEach((id) => {
      const collapse = document.getElementById(id);
      if (collapse) {
        collapse.classList.add("show");
      }
    });
  }

  // Thiết lập auto-save
  setupAutoSave() {
    // Lưu khi có thay đổi form
    document.addEventListener("input", () => {
      this.debounce(() => this.saveState(), 1000);
    });

    // Lưu khi có thay đổi UI
    document.addEventListener("click", (e) => {
      if (e.target.matches(".nav-link, .btn, .collapse-toggle")) {
        this.debounce(() => this.saveState(), 500);
      }
    });
  }

  // Debounce function
  debounce(func, wait) {
    clearTimeout(this.debounceTimer);
    this.debounceTimer = setTimeout(func, wait);
  }

  // Xóa state
  clearState() {
    localStorage.removeItem(this.storageKey);
    console.log("State cleared");
  }

  // Lấy state hiện tại
  getCurrentState() {
    const savedState = localStorage.getItem(this.storageKey);
    return savedState ? JSON.parse(savedState) : null;
  }
}

// Khởi tạo StateManager khi DOM ready
document.addEventListener("DOMContentLoaded", () => {
  window.stateManager = new StateManager();
});

// Export cho module systems
if (typeof module !== "undefined" && module.exports) {
  module.exports = StateManager;
}



