$(function () {
  const $msg = $("#msg");
  const $box = $("#profile");

  if (!requireLogin()) return;
  renderNav("books");

  const id = new URLSearchParams(location.search).get("id");
  if (!id) {
    showError("No author selected.");
    return;
  }
  loadProfile(id);

  // ---------- AJAX ----------
  function loadProfile(authorId) {
    $box.html('<div class="empty">Loading…</div>');

    $.ajax({
      url:
        API_BASE + "/api/authors/" + encodeURIComponent(authorId) + "/profile",
      method: "GET",
      dataType: "json",
      headers: { Authorization: "Bearer " + Session.token() },
    })
      .done(renderProfile)
      .fail(function (xhr) {
        if (xhr.status === 401) {
          Session.clear();
          location.href = "index.html?expired=1";
          return;
        }
        $box.empty();
        showError(
          xhr.status === 0
            ? "Cannot reach the server. Is the API running?"
            : extractMessage(xhr.responseJSON, xhr.status),
        );
      });
  }

  // ---------- helpers ----------
  function showError(text) {
    $msg.empty().append($("<div>").addClass("msg error").text(text));
  }

  function hueOf(name) {
    let h = 0;
    for (const ch of name) h = (h * 31 + ch.charCodeAt(0)) % 360;
    return h;
  }

  function initials(name) {
    return name
      .split(" ")
      .filter(Boolean)
      .slice(0, 2)
      .map((w) => w[0])
      .join("")
      .toUpperCase();
  }

  function pill(text) {
    return $("<span>").addClass("pill").text(text);
  }

  function stat(value, label) {
    const $n = $("<div>").addClass("stat-n").text(value);
    if (typeof value === "number") $n.data("target", value).text("0");
    return $("<div>")
      .addClass("stat")
      .append($n, $("<div>").addClass("muted").text(label));
  }

  function countUp($el) {
    const to = $el.data("target");
    $({ n: 0 }).animate(
      { n: to },
      {
        duration: 700,
        step: (now) => $el.text(Math.round(now)),
        complete: () => $el.text(to),
      },
    );
  }

  // ---------- render ----------
  function renderProfile(a) {
    document.title = a.name + " – Library";

    const totalCopies = a.books.reduce((s, b) => s + b.totalCopies, 0);
    const available = a.books.reduce((s, b) => s + b.availableCopies, 0);
    const firstYear = a.books.length
      ? Math.min(...a.books.map((b) => b.publishYear))
      : "—";
    const born = a.birthDate
      ? new Date(a.birthDate).toLocaleDateString(undefined, {
          year: "numeric",
          month: "long",
          day: "numeric",
        })
      : null;

    const $header = $("<div>")
      .addClass("profile-head")
      .append(
        $("<div>")
          .addClass("avatar")
          .css("background", `hsl(${hueOf(a.name)},55%,42%)`)
          .text(initials(a.name)),
        $("<div>").append(
          $("<h1>").addClass("profile-name").text(a.name),
          $("<div>")
            .addClass("meta")
            .append(
              ...[
                a.nationality && pill("🌍 " + a.nationality),
                born && pill("🎂 Born " + born),
                pill(
                  `📚 ${a.booksCount} ${a.booksCount === 1 ? "book" : "books"}`,
                ),
              ].filter(Boolean),
            ),
        ),
      );

    const $stats = $("<div>")
      .addClass("stats")
      .append(
        stat(a.booksCount, "Books"),
        stat(totalCopies, "Copies in library"),
        stat(available, "Available now"),
        stat(firstYear, "First published"),
      );

    const $about = $("<section>").append(
      $("<h3>").text("About"),
      $("<p>")
        .addClass("bio")
        .text(a.bio || "No biography available."),
    );

    const $books = $("<section>").append($("<h3>").text(`Books by ${a.name}`));

    if (a.books.length === 0) {
      $books.append($("<div>").addClass("empty").text("No books yet."));
    } else {
      const $filter = $("<input>")
        .attr({ type: "search", placeholder: "Filter these books…" })
        .addClass("book-filter");
      const $none = $("<div>")
        .addClass("empty")
        .text("No matching books.")
        .hide();
      const $grid = $("<div>").addClass("book-grid");

      $.each(a.books, function (_, b) {
        $grid.append(
          $("<div>")
            .addClass("book-card")
            .append(
              $("<div>").addClass("book-title").text(b.title),
              $("<div>")
                .addClass("muted")
                .text("ISBN " + b.isbn),
              $("<div>")
                .addClass("book-meta")
                .append(
                  $("<span>").addClass("badge gray").text(b.categoryName),
                  $("<span>").addClass("muted").text(b.publishYear),
                ),
              $("<span>")
                .addClass("badge " + (b.availableCopies > 0 ? "ok" : "bad"))
                .text(
                  b.availableCopies > 0
                    ? `${b.availableCopies} of ${b.totalCopies} available`
                    : "Unavailable",
                ),
            ),
        );
      });

      $filter.on("input", function () {
        const q = $(this).val().trim().toLowerCase();
        let visible = 0;
        $grid.children(".book-card").each(function () {
          const show = $(this).text().toLowerCase().includes(q);
          $(this).toggle(show);
          if (show) visible++;
        });
        $none.toggle(visible === 0);
      });

      $books.append($filter, $grid, $none);
    }

    $box
      .hide()
      .empty()
      .append(
        $("<a>")
          .attr("href", "books.html")
          .addClass("back")
          .text("← Back to books"),
        $header,
        $stats,
        $about,
        $books,
      )
      .fadeIn(250);

    $stats.find(".stat-n").each(function () {
      if ($(this).data("target") !== undefined) countUp($(this));
    });
  }
});
