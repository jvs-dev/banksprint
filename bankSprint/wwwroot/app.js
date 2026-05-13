/**
 * Mesmo hostname da pagina (localhost vs 127.0.0.1).
 */
const API_BASE = `http://${window.location.hostname}:5121`;

const TOKEN_KEY = "banking_jwt";
const TRANSFER_RECENTS_KEY = "bankSprint_transfer_recents";

let currentUserAccountId = null;

const el = (id) => document.getElementById(id);

function getToken() {
  return localStorage.getItem(TOKEN_KEY);
}

function setToken(token) {
  if (token) localStorage.setItem(TOKEN_KEY, token);
  else localStorage.removeItem(TOKEN_KEY);
}

function isLoggedIn() {
  return Boolean(getToken());
}

function authHeaders() {
  const token = getToken();
  const headers = { "Content-Type": "application/json" };
  if (token) headers.Authorization = `Bearer ${token}`;
  return headers;
}

/** Atualiza header (Login/Cadastro vs Sair) e visibilidade dos paineis. */
function applyAuthView() {
  const logged = isLoggedIn();
  const panelAuth = el("panel-auth");
  const panelBanking = el("panel-banking");
  const navGuest = el("nav-buttons-guest");
  const navUser = el("nav-buttons-user");

  if (logged) {
    panelAuth.classList.add("d-none");
    panelBanking.classList.remove("d-none");
    navGuest.classList.add("d-none");
    navUser.classList.remove("d-none");
  } else {
    panelAuth.classList.remove("d-none");
    panelBanking.classList.add("d-none");
    navGuest.classList.remove("d-none");
    navUser.classList.add("d-none");
    currentUserAccountId = null;
    const sec = el("section-transfer");
    if (sec) sec.classList.add("d-none");
  }
}

function showTabLogin() {
  const tab = el("tab-login");
  if (tab) bootstrap.Tab.getOrCreateInstance(tab).show();
}

function showTabRegister() {
  const tab = el("tab-register");
  if (tab) bootstrap.Tab.getOrCreateInstance(tab).show();
}

function hideBootstrapModal(modalId) {
  const node = document.getElementById(modalId);
  if (!node) return;
  const instance = bootstrap.Modal.getInstance(node);
  if (instance) instance.hide();
}

/**
 * sistema de notificações
 * @param {string} message - Mensagem a exibir
 * @param {string} type - Tipo de notificação (success, danger, warning, info, secondary)
 * @param {number} duration - Duração em ms (0 = manual close)
 */
function showAlert(message, type = "info", duration = 6000) {
  let container = document.getElementById("notification-container");
  
  // Se o container não existe, criar um
  if (!container) {
    container = document.createElement("div");
    container.id = "notification-container";
    container.className = "notification-container";
    document.body.appendChild(container);
  }
  
  const notification = document.createElement("div");
  notification.className = `notification ${type}`;
  
  // Mapear tipos de alerta para ícones
  const icons = {
    success: '<i class="bi bi-check-circle-fill"></i>',
    danger: '<i class="bi bi-exclamation-circle-fill"></i>',
    warning: '<i class="bi bi-exclamation-triangle-fill"></i>',
    info: '<i class="bi bi-info-circle-fill"></i>',
    secondary: '<i class="bi bi-chat-dot-fill"></i>'
  };
  
  notification.innerHTML = `
    <div class="notification-icon">${icons[type] || icons.info}</div>
    <div class="notification-content">${message}</div>
    <button class="notification-close" aria-label="Fechar">
      <i class="bi bi-x-lg"></i>
    </button>
  `;
  
  // Close button
  notification.querySelector(".notification-close").addEventListener("click", () => {
    removeNotification(notification);
  });
  
  container.appendChild(notification);
  
  // Auto-remove após duration
  if (duration > 0) {
    setTimeout(() => {
      removeNotification(notification);
    }, duration);
  }
}

/**
 * Remove notificação com animação
 */
function removeNotification(notification) {
  if (!notification.parentNode) return;
  notification.classList.add("removing");
  setTimeout(() => {
    if (notification.parentNode) {
      notification.parentNode.removeChild(notification);
    }
  }, 300);
}

/**
 * Adiciona estado de loading num botão
 */
function setButtonLoading(button, isLoading) {
  if (isLoading) {
    button.disabled = true;
    button.classList.add("loading");
    button.dataset.originalText = button.innerHTML;
  } else {
    button.disabled = false;
    button.classList.remove("loading");
    if (button.dataset.originalText) {
      button.innerHTML = button.dataset.originalText;
    }
  }
}

async function parseJsonSafe(response) {
  const text = await response.text();
  if (!text) return null;
  try {
    return JSON.parse(text);
  } catch {
    return { raw: text };
  }
}

async function apiFetch(path, options = {}) {
  const url = `${API_BASE}${path}`;
  const res = await fetch(url, {
    ...options,
    headers: { ...authHeaders(), ...(options.headers || {}) },
  });
  const body = await parseJsonSafe(res);
  return { res, body };
}

function formatMoney(value) {
  return new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" }).format(Number(value) || 0);
}

/** Moeda com sinal explícito (+R$ / -R$), para transferências no extrato. */
function formatSignedCurrency(value) {
  const n = Number(value);
  if (Number.isNaN(n)) return "—";
  return new Intl.NumberFormat("pt-BR", {
    style: "currency",
    currency: "BRL",
    signDisplay: "exceptZero",
  }).format(n);
}

function formatDate(iso) {
  if (!iso) return "—";
  const d = new Date(iso);
  if (Number.isNaN(d.getTime())) return iso;
  return d.toLocaleString("pt-BR");
}

function transactionTypeLabel(t) {
  if (t === 0 || t === "Deposit") return "Depósito";
  if (t === 1 || t === "Withdraw") return "Saque";
  if (t === 2 || t === "Transfer") return "Transferência";
  return String(t);
}

function isTransferType(t) {
  return t === 2 || t === "Transfer";
}

function formatStatementAmount(t) {
  if (isTransferType(t.type)) return formatSignedCurrency(t.amount);
  return formatMoney(t.amount);
}

async function refreshStatement() {
  const { res, body } = await apiFetch("/api/transactions", { method: "GET" });
  if (res.status === 401) {
    setToken(null);
    applyAuthView();
    showAlert("Sessão expirada ou não autorizado. Faça login novamente.", "warning");
    return;
  }
  if (!res.ok) {
    showAlert(body?.erro || `Erro ao carregar extrato (${res.status})`, "danger");
    return;
  }

  const account = body.account;
  currentUserAccountId = account.id;
  el("user-summary").textContent = `${account.name} · ${account.email} · ${account.role}`;
  el("balance-display").textContent = formatMoney(account.balance);

  renderTransferRecents();

  const tbody = el("statement-body");
  tbody.innerHTML = "";
  const txs = body.transactions || [];
  if (txs.length === 0) {
    tbody.innerHTML =
      '<tr><td colspan="3" class="text-center text-muted py-4">Sem movimentações</td></tr>';
    return;
  }
  for (const t of txs) {
    const tr = document.createElement("tr");
    tr.innerHTML = `
      <td>${formatDate(t.date)}</td>
      <td>${transactionTypeLabel(t.type)}</td>
      <td class="text-end">${formatStatementAmount(t)}</td>
    `;
    tbody.appendChild(tr);
  }
}

function loadTransferRecents() {
  try {
    const raw = localStorage.getItem(TRANSFER_RECENTS_KEY);
    if (!raw) return [];
    const arr = JSON.parse(raw);
    if (!Array.isArray(arr)) return [];
    return arr
      .map((x) => parseInt(String(x), 10))
      .filter((n) => Number.isInteger(n) && n > 0);
  } catch {
    return [];
  }
}

function saveTransferRecentId(accountId) {
  const id = Number(accountId);
  if (!Number.isInteger(id) || id <= 0) return;
  let list = loadTransferRecents().filter((x) => x !== id);
  list.unshift(id);
  list = list.slice(0, 8);
  localStorage.setItem(TRANSFER_RECENTS_KEY, JSON.stringify(list));
}

function renderTransferRecents() {
  const wrap = el("transfer-recents-wrap");
  const listEl = el("transfer-recents-list");
  if (!wrap || !listEl) return;

  const selfId = currentUserAccountId;
  const ids = loadTransferRecents().filter((id) => id !== selfId);

  listEl.innerHTML = "";
  if (ids.length === 0) {
    wrap.classList.add("d-none");
    return;
  }

  wrap.classList.remove("d-none");
  for (const id of ids) {
    const b = document.createElement("button");
    b.type = "button";
    b.className = "btn btn-outline-secondary btn-sm rounded-pill btn-recent-id";
    b.textContent = `ID ${id}`;
    b.addEventListener("click", () => {
      el("transfer-to-account-id").value = String(id);
    });
    listEl.appendChild(b);
  }
}

function logoutToLogin() {
  setToken(null);
  el("login-password").value = "";
  applyAuthView();
  showTabLogin();
  showAlert("Você saiu da conta.", "secondary");
}

el("header-btn-login").addEventListener("click", () => {
  showTabLogin();
});

el("header-btn-register").addEventListener("click", () => {
  showTabRegister();
});

el("header-btn-logout").addEventListener("click", () => {
  logoutToLogin();
});

el("form-login").addEventListener("submit", async (e) => {
  e.preventDefault();
  const email = el("login-email").value.trim();
  const password = el("login-password").value;
  const button = e.target.querySelector("button[type=submit]");
  
  setButtonLoading(button, true);
  
  try {
    const res = await fetch(`${API_BASE}/api/auth/login`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ email, password }),
    });
    const body = await parseJsonSafe(res);
    if (res.status === 401) {
      showAlert(body?.erro || "E-mail ou senha inválidos.", "danger");
      return;
    }
    if (!res.ok) {
      showAlert(body?.erro || `Erro no login (${res.status})`, "danger");
      return;
    }
    setToken(body.token);
    showAlert(body.message || "Login realizado com sucesso!", "success");
    applyAuthView();
    await refreshStatement();
  } finally {
    setButtonLoading(button, false);
  }
});

el("form-register").addEventListener("submit", async (e) => {
  e.preventDefault();
  const name = el("register-name").value.trim();
  const email = el("register-email").value.trim();
  const password = el("register-password").value;
  const button = e.target.querySelector("button[type=submit]");
  
  setButtonLoading(button, true);
  
  try {
    const res = await fetch(`${API_BASE}/api/auth/register`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ name, email, password }),
    });
    const body = await parseJsonSafe(res);
    if (res.status === 400) {
      showAlert(body?.erro || "Dados inválidos.", "danger");
      return;
    }
    if (!res.ok) {
      const extra = body?.detalhe ? ` (${body.detalhe})` : "";
      showAlert((body?.erro || `Erro no cadastro (${res.status})`) + extra, "danger");
      return;
    }
    showAlert("Cadastro concluído com sucesso! Faça login com o e-mail e a senha informados.", "success");
    showTabLogin();
    el("login-email").value = email;
    el("register-name").value = "";
    el("register-email").value = "";
    el("register-password").value = "";
  } finally {
    setButtonLoading(button, false);
  }
});

el("btn-refresh").addEventListener("click", async () => {
  const btn = el("btn-refresh");
  const icon = btn.querySelector("i");
  const originalIconClass = icon.className;
  
  // Adiciona animação de rotação
  icon.style.animation = "spin 0.6s linear";
  btn.disabled = true;
  
  try {
    await refreshStatement();
  } finally {
    icon.style.animation = "";
    btn.disabled = false;
  }
});

el("btn-historico").addEventListener("click", () => {
  const sec = el("section-transfer");
  if (sec) sec.classList.add("d-none");
  el("section-historico").scrollIntoView({ behavior: "smooth", block: "start" });
});

el("btn-transfer-nav").addEventListener("click", () => {
  const sec = el("section-transfer");
  if (!sec) return;
  sec.classList.remove("d-none");
  renderTransferRecents();
  sec.scrollIntoView({ behavior: "smooth", block: "start" });
});

el("form-deposit").addEventListener("submit", async (e) => {
  e.preventDefault();
  const amount = Number(el("deposit-amount").value);
  const button = e.target.querySelector("button[type=submit]");
  
  setButtonLoading(button, true);
  
  try {
    const { res, body } = await apiFetch("/api/transactions/deposit", {
      method: "POST",
      body: JSON.stringify({ amount }),
    });
    if (res.status === 401) {
      setToken(null);
      applyAuthView();
      showTabLogin();
      showAlert("Sessão expirada. Faça login novamente.", "warning");
      return;
    }
    if (!res.ok) {
      showAlert(body?.erro || `Erro no depósito (${res.status})`, "danger");
      return;
    }
    showAlert("Depósito realizado com sucesso!", "success");
    el("deposit-amount").value = "";
    hideBootstrapModal("modalDeposit");
    await refreshStatement();
  } finally {
    setButtonLoading(button, false);
  }
});

el("form-withdraw").addEventListener("submit", async (e) => {
  e.preventDefault();
  const amount = Number(el("withdraw-amount").value);
  const button = e.target.querySelector("button[type=submit]");
  
  setButtonLoading(button, true);
  
  try {
    const { res, body } = await apiFetch("/api/transactions/withdraw", {
      method: "POST",
      body: JSON.stringify({ amount }),
    });
    if (res.status === 401) {
      setToken(null);
      applyAuthView();
      showTabLogin();
      showAlert("Sessão expirada. Faça login novamente.", "warning");
      return;
    }
    if (!res.ok) {
      showAlert(body?.erro || `Erro no saque (${res.status})`, "danger");
      return;
    }
    showAlert("Saque realizado com sucesso!", "success");
    el("withdraw-amount").value = "";
    hideBootstrapModal("modalWithdraw");
    await refreshStatement();
  } finally {
    setButtonLoading(button, false);
  }
});

el("form-transfer").addEventListener("submit", async (e) => {
  e.preventDefault();
  const toId = parseInt(el("transfer-to-account-id").value, 10);
  const amount = Number(el("transfer-amount").value);
  const button = e.target.querySelector("button[type=submit]");

  if (!Number.isInteger(toId) || toId < 1) {
    showAlert("Informe um ID de conta de destino válido.", "warning");
    return;
  }
  if (currentUserAccountId != null && toId === currentUserAccountId) {
    showAlert("Não é possível transferir para a própria conta.", "warning");
    return;
  }

  setButtonLoading(button, true);
  
  try {
    const { res, body } = await apiFetch("/api/transactions/transfer", {
      method: "POST",
      body: JSON.stringify({ toAccountId: toId, amount }),
    });
    if (res.status === 401) {
      setToken(null);
      applyAuthView();
      showTabLogin();
      showAlert("Sessão expirada. Faça login novamente.", "warning");
      return;
    }
    if (res.status === 404) {
      showAlert(body?.erro || "Conta de destino não encontrada.", "danger");
      return;
    }
    if (!res.ok) {
      showAlert(body?.erro || `Erro na transferência (${res.status})`, "danger");
      return;
    }
    saveTransferRecentId(toId);
    renderTransferRecents();
    showAlert("Transferência realizada com sucesso!", "success");
    el("transfer-amount").value = "";
    await refreshStatement();
  } finally {
    setButtonLoading(button, false);
  }
});

window.addEventListener("DOMContentLoaded", () => {
  applyAuthView();
  if (isLoggedIn()) {
    refreshStatement().catch(() => {
      setToken(null);
      applyAuthView();
    });
  }
});
