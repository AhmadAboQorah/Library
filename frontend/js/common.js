// ---------- shared helpers used by every page ----------
const TOKEN_KEY = "libraryToken";
const USER_KEY = "libraryUser";

const Session = {
  token: () => localStorage.getItem(TOKEN_KEY),
  user() {
    try {
      return JSON.parse(localStorage.getItem(USER_KEY));
    } catch {
      return null;
    }
  },
  save(token, user) {
    localStorage.setItem(TOKEN_KEY, token);
    localStorage.setItem(USER_KEY, JSON.stringify(user));
  },
  clear() {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
  },
  isLoggedIn: () => !!localStorage.getItem(TOKEN_KEY),
  isAdmin() {
    const u = this.user();
    return !!u && u.role === "Admin";
  },
  isUser() {
    const u = this.user();
    return !!u && u.role === "User";
  },
};

class ApiError extends Error {
  constructor(message, status) {
    super(message);
    this.status = status;
  }
}

// Turns whatever the API returned into one readable sentence
function extractMessage(data, status) {
  if (data) {
    if (data.message) return data.message;
    if (data.errors) return Object.values(data.errors).flat().join(" ");
    if (data.title) return data.title;
  }
  if (status === 401) return "Please log in first.";
  if (status === 403) return "You don't have permission to do this.";
  if (status === 404) return "Not found.";
  return "Something went wrong (" + status + ").";
}

// Every request goes through here: Fetch + async/await + Authorization header
async function api(path, { method = "GET", body, protectedCall = true } = {}) {
  const headers = {};
  if (body !== undefined) headers["Content-Type"] = "application/json";
  const token = Session.token();
  if (token) headers["Authorization"] = "Bearer " + token;

  let res;
  try {
    res = await fetch(API_BASE + path, {
      method,
      headers,
      body: body !== undefined ? JSON.stringify(body) : undefined,
    });
  } catch {
    throw new ApiError("Cannot reach the server. Is the API running?", 0);
  }

  // expired / missing token -> back to the login page
  if (res.status === 401 && protectedCall && token) {
    Session.clear();
    location.href = "index.html?expired=1";
    throw new ApiError("Session expired.", 401);
  }

  if (res.status === 204) return null;
  let data = null;
  try {
    data = await res.json();
  } catch {
    /* empty body */
  }
  if (!res.ok) throw new ApiError(extractMessage(data, res.status), res.status);
  return data;
}

// ---------- tiny DOM helper: el("div", {class:"x", onclick:fn}, child1, "text") ----------
function el(tag, props = {}, ...children) {
  const node = document.createElement(tag);
  for (const [k, v] of Object.entries(props)) {
    if (k === "class") node.className = v;
    else if (k.startsWith("on")) node.addEventListener(k.slice(2), v);
    else if (v !== undefined && v !== null && v !== false)
      node.setAttribute(k, v === true ? "" : v);
  }
  for (const c of children.flat()) {
    if (c === null || c === undefined || c === false) continue;
    node.append(c.nodeType ? c : document.createTextNode(String(c)));
  }
  return node;
}

function showMessage(container, text, type = "error") {
  container.replaceChildren();
  if (!text) return;
  container.append(el("div", { class: "msg " + type }, text));
}

function fmtDate(iso) {
  if (!iso) return "—";
  return new Date(iso).toLocaleDateString();
}

function logout() {
  Session.clear();
  location.href = "index.html";
}

// ---------- top navigation (built with createElement) ----------
let navDocHandlers = null;

function navHue(name) {
  let h = 0;
  for (const ch of name) h = (h * 31 + ch.charCodeAt(0)) % 360;
  return h;
}

function navInitials(name) {
  return name
    .split(" ")
    .filter(Boolean)
    .slice(0, 2)
    .map((w) => w[0])
    .join("")
    .toUpperCase();
}

function renderNav(active) {
  if (navDocHandlers) {
    document.removeEventListener("click", navDocHandlers.click);
    document.removeEventListener("keydown", navDocHandlers.key);
    navDocHandlers = null;
  }

  const nav = document.getElementById("nav");
  nav.replaceChildren();
  nav.className = "nav";
  const link = (href, text, key) =>
    el(
      "a",
      { href, class: "nav-link" + (key === active ? " active" : "") },
      text,
    );

  const links = el(
    "div",
    { class: "nav-links" },
    link("books.html", "Books", "books"),
  );
  if (Session.isUser()) links.append(link("loans.html", "My loans", "loans"));
  if (Session.isAdmin()) {
    links.append(
      link("loans.html", "Overdue loans", "loans"),
      link("admin.html", "Authors & categories", "admin"),
    );
  }

  nav.append(
    el(
      "a",
      { href: "books.html", class: "brand" },
      el("span", {}, "📚"),
      "Library",
    ),
    links,
    el("span", { class: "spacer" }),
  );

  nav.append(
    Session.isLoggedIn()
      ? userMenu(active)
      : link("index.html", "Login / Register", "login"),
  );
}

function userMenu(active) {
  const u = Session.user();

  const trigger = el(
    "button",
    {
      type: "button",
      class: "user-chip" + (active === "profile" ? " active" : ""),
      "aria-haspopup": "true",
      "aria-expanded": "false",
    },
    el(
      "span",
      {
        class: "nav-avatar",
        style: `background:hsl(${navHue(u.fullName)},55%,42%)`,
      },
      navInitials(u.fullName),
    ),
    el(
      "span",
      { class: "user-text" },
      el("span", { class: "user-name" }, u.fullName),
      el("span", { class: "user-role" }, u.role),
    ),
    el("span", { class: "caret" }, "▾"),
  );

  const menu = el(
    "div",
    { class: "user-menu", hidden: true },
    el(
      "div",
      { class: "user-menu-head" },
      el("strong", {}, u.fullName),
      el("div", { class: "muted" }, u.email || ""),
    ),
    el("a", { href: "profile.html", class: "menu-item" }, "👤  My profile"),
    el(
      "button",
      { type: "button", class: "menu-item danger-item", onclick: logout },
      "⎋  Logout",
    ),
  );

  const wrap = el("div", { class: "user-menu-wrap" }, trigger, menu);

  const setOpen = (open) => {
    menu.hidden = !open;
    trigger.setAttribute("aria-expanded", String(open));
  };
  trigger.addEventListener("click", (e) => {
    e.stopPropagation();
    setOpen(menu.hidden);
  });

  navDocHandlers = {
    click: (e) => {
      if (!wrap.contains(e.target)) setOpen(false);
    },
    key: (e) => {
      if (e.key === "Escape") setOpen(false);
    },
  };
  document.addEventListener("click", navDocHandlers.click);
  document.addEventListener("keydown", navDocHandlers.key);

  return wrap;
}

// Pages that need a logged-in user call this first
function requireLogin(role) {
  if (!Session.isLoggedIn() || (role && Session.user().role !== role)) {
    location.href = "index.html";
    return false;
  }
  return true;
}
