const msg = document.getElementById("msg");
const listBox = document.getElementById("list");
const searchInput = document.getElementById("search");
const categoryFilter = document.getElementById("categoryFilter");
const addBtn = document.getElementById("addBtn");
const pager = document.getElementById("pager");

let categories = [];
let authors = [];
let currentPage = 1;
const PAGE_SIZE = 10;

renderNav("books");

// ---------- load data ----------
async function loadLookups() {
  try {
    [categories, authors] = await Promise.all([
      api("/api/categories"),
      api("/api/authors"),
    ]);
    for (const c of categories)
      categoryFilter.append(el("option", { value: c.id }, c.name));
  } catch (err) {
    showMessage(msg, err.message);
  }
}

async function loadBooks(page = 1) {
  currentPage = page;
  const params = new URLSearchParams();
  if (searchInput.value.trim()) params.set("search", searchInput.value.trim());
  if (categoryFilter.value) params.set("categoryId", categoryFilter.value);
  params.set("page", currentPage);
  params.set("pageSize", PAGE_SIZE);

  try {
    const result = await api("/api/books?" + params.toString());
    renderBooks(result.items);
    renderPager(result);
  } catch (err) {
    showMessage(msg, err.message);
  }
}

function renderPager(r) {
  pager.replaceChildren();
  if (r.totalPages <= 1) return;
  pager.append(
    el(
      "button",
      {
        class: "small secondary",
        disabled: r.page <= 1,
        onclick: () => loadBooks(r.page - 1),
      },
      "Previous",
    ),
    el(
      "span",
      { class: "muted" },
      ` Page ${r.page} of ${r.totalPages} (${r.totalCount} books) `,
    ),
    el(
      "button",
      {
        class: "small secondary",
        disabled: r.page >= r.totalPages,
        onclick: () => loadBooks(r.page + 1),
      },
      "Next",
    ),
  );
}

// ---------- render ----------
function renderBooks(books) {
  listBox.replaceChildren();
  if (books.length === 0) {
    listBox.append(el("div", { class: "empty" }, "No books found."));
    return;
  }

  const head = el(
    "tr",
    {},
    ...["Title", "Authors", "Category", "Year", "Available", ""].map((h) =>
      el("th", {}, h),
    ),
  );
  const table = el("table", {}, el("thead", {}, head));
  const body = el("tbody");

  for (const b of books) {
    const actions = el("div", { class: "actions" });
    if (Session.isUser()) {
      actions.append(
        el(
          "button",
          {
            class: "small",
            disabled: b.availableCopies === 0,
            onclick: () => borrow(b),
          },
          b.availableCopies === 0 ? "Unavailable" : "Borrow",
        ),
      );
    }
    if (Session.isAdmin()) {
      actions.append(
        el(
          "button",
          { class: "small secondary", onclick: () => openForm(b) },
          "Edit",
        ),
        el(
          "button",
          { class: "small danger", onclick: () => removeBook(b) },
          "Delete",
        ),
      );
    }

    body.append(
      el(
        "tr",
        {},
        el(
          "td",
          {},
          el("strong", {}, b.title),
          el("div", { class: "muted" }, "ISBN " + b.isbn),
        ),
        el(
          "td",
          {},
          b.authors.flatMap((a, i) => [
            i > 0 ? ", " : null,
            el("a", { href: "author.html?id=" + a.id }, a.name),
          ]),
        ),
        el("td", {}, b.categoryName),
        el("td", {}, b.publishYear),
        el(
          "td",
          {},
          el(
            "span",
            { class: "badge " + (b.availableCopies > 0 ? "ok" : "bad") },
            `${b.availableCopies} / ${b.totalCopies}`,
          ),
        ),
        el("td", {}, actions),
      ),
    );
  }
  table.append(body);
  listBox.append(table);
}

// ---------- actions ----------
async function borrow(book) {
  try {
    const loan = await api("/api/loans", {
      method: "POST",
      body: { bookId: book.id },
    });
    showMessage(
      msg,
      `You borrowed "${book.title}". Return it before ${fmtDate(loan.dueDate)}.`,
      "success",
    );
    loadBooks(currentPage);
  } catch (err) {
    showMessage(msg, err.message);
  }
}

async function removeBook(book) {
  if (!confirm(`Delete "${book.title}"?`)) return;
  try {
    await api("/api/books/" + book.id, { method: "DELETE" });
    showMessage(msg, "Book deleted.", "success");
    loadBooks(currentPage);
  } catch (err) {
    showMessage(msg, err.message);
  }
}

// ---------- searchable multi-select for authors ----------
function createAuthorPicker(all, selectedIds = []) {
  const selected = new Map(
    all.filter((a) => selectedIds.includes(a.id)).map((a) => [a.id, a]),
  );
  const chips = el("div", { class: "chips" });
  const input = el("input", {
    type: "text",
    placeholder: "Type to search authors…",
    autocomplete: "off",
  });
  const list = el("div", { class: "picker-list", hidden: true });
  const node = el("div", { class: "picker" }, chips, input, list);

  function renderChips() {
    chips.replaceChildren(
      ...[...selected.values()].map((a) =>
        el(
          "span",
          { class: "chip" },
          a.name,
          el(
            "button",
            {
              type: "button",
              class: "chip-x",
              title: "Remove",
              onclick: () => {
                selected.delete(a.id);
                renderChips();
                renderList();
              },
            },
            "×",
          ),
        ),
      ),
    );
  }

  function renderList() {
    const q = input.value.trim().toLowerCase();
    const matches = all.filter(
      (a) => !selected.has(a.id) && a.name.toLowerCase().includes(q),
    );
    list.replaceChildren(
      ...(matches.length
        ? matches.map((a) =>
            el(
              "div",
              {
                class: "picker-item",
                onmousedown: (e) => {
                  e.preventDefault();
                  selected.set(a.id, a);
                  input.value = "";
                  renderChips();
                  renderList();
                  input.focus();
                },
              },
              a.name,
              a.nationality
                ? el("span", { class: "muted" }, " · " + a.nationality)
                : null,
            ),
          )
        : [el("div", { class: "picker-empty" }, "No authors found")]),
    );
  }

  input.addEventListener("focus", () => {
    list.hidden = false;
    renderList();
  });
  input.addEventListener("input", renderList);
  input.addEventListener("blur", () => {
    list.hidden = true;
  });

  renderChips();
  return { node, getIds: () => [...selected.keys()] };
}

// ---------- add / edit form (Admin only) ----------
function openForm(book) {
  const editing = !!book;
  const formMsg = el("div");
  const f = {
    title: el("input", {
      type: "text",
      required: true,
      value: book?.title ?? "",
    }),
    isbn: el("input", {
      type: "text",
      required: true,
      minlength: 10,
      maxlength: 20,
      value: book?.isbn ?? "",
    }),
    year: el("input", {
      type: "number",
      required: true,
      min: 1000,
      max: 2100,
      value: book?.publishYear ?? new Date().getFullYear(),
    }),
    copies: el("input", {
      type: "number",
      required: true,
      min: 1,
      value: book?.totalCopies ?? 1,
    }),
    category: el(
      "select",
      { required: true },
      el("option", { value: "" }, "Select category"),
      categories.map((c) =>
        el(
          "option",
          { value: c.id, selected: book?.categoryId === c.id },
          c.name,
        ),
      ),
    ),
  };
  const picker = createAuthorPicker(
    authors,
    book?.authors.map((x) => x.id) ?? [],
  );

  const close = () => bg.remove();
  const form = el(
    "form",
    {},
    el("h2", {}, editing ? "Edit book" : "Add book"),
    formMsg,
    el("label", {}, "Title"),
    f.title,
    el("label", {}, "ISBN"),
    f.isbn,
    el("label", {}, "Publish year"),
    f.year,
    el("label", {}, "Total copies"),
    f.copies,
    el("label", {}, "Category"),
    f.category,
    el("label", {}, "Authors"),
    picker.node,
    el(
      "p",
      { class: "row" },
      el("button", { type: "submit" }, "Save"),
      el(
        "button",
        { type: "button", class: "secondary", onclick: close },
        "Cancel",
      ),
    ),
  );

  form.addEventListener("submit", async (e) => {
    e.preventDefault();
    const authorIds = picker.getIds();
    if (authorIds.length === 0) {
      showMessage(formMsg, "Select at least one author.");
      return;
    }

    const payload = {
      title: f.title.value,
      isbn: f.isbn.value,
      publishYear: Number(f.year.value),
      totalCopies: Number(f.copies.value),
      categoryId: Number(f.category.value),
      authorIds,
    };
    try {
      await api(editing ? "/api/books/" + book.id : "/api/books", {
        method: editing ? "PUT" : "POST",
        body: payload,
      });
      close();
      showMessage(msg, editing ? "Book updated." : "Book added.", "success");
      loadBooks(editing ? currentPage : 1);
    } catch (err) {
      showMessage(formMsg, err.message);
    }
  });

  const bg = el(
    "div",
    { class: "modal-bg" },
    el("div", { class: "modal" }, form),
  );
  document.body.append(bg);
}

// ---------- wire up ----------
document
  .getElementById("searchBtn")
  .addEventListener("click", () => loadBooks(1));
searchInput.addEventListener("keydown", (e) => {
  if (e.key === "Enter") loadBooks(1);
});
categoryFilter.addEventListener("change", () => loadBooks(1));
document.getElementById("resetBtn").addEventListener("click", () => {
  searchInput.value = "";
  categoryFilter.value = "";
  loadBooks(1);
});

if (Session.isAdmin()) {
  addBtn.hidden = false;
  addBtn.addEventListener("click", () => openForm(null));
}

(async () => {
  await loadLookups();
  await loadBooks();
})();
