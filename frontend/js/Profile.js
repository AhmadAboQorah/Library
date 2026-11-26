$(function () {
  if (!requireLogin()) return;
  renderNav("profile");

  const $msg = $("#msg");
  const $box = $("#profile");

  load();

  // ---------- AJAX ----------
  function call(method, path, body) {
    const req = $.ajax({
      url: API_BASE + path,
      method: method,
      contentType: "application/json",
      data: body !== undefined ? JSON.stringify(body) : undefined,
      headers: { Authorization: "Bearer " + Session.token() },
    });
    req.fail(function (xhr) {
      if (xhr.status === 401) {
        Session.clear();
        location.href = "index.html?expired=1";
      }
    });
    return req;
  }

  function errorText(xhr) {
    return xhr.status === 0
      ? "Cannot reach the server. Is the API running?"
      : extractMessage(xhr.responseJSON, xhr.status);
  }

  function load() {
    $box.html('<div class="card empty">Loading…</div>');
    call("GET", "/api/users/me")
      .done(render)
      .fail(function (xhr) {
        $box.empty();
        $msg
          .empty()
          .append($("<div>").addClass("msg error").text(errorText(xhr)));
      });
  }

  // ---------- small helpers ----------
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
    return $("<div>")
      .addClass("stat")
      .append(
        $("<div>").addClass("stat-n").text(value),
        $("<div>").addClass("muted").text(label),
      );
  }
  function field(labelText, $input) {
    return $("<div>").append($("<label>").text(labelText), $input);
  }
  function flash($el, text, type) {
    $el
      .stop(true, true)
      .empty()
      .append(
        $("<div>")
          .addClass("msg " + type)
          .text(text),
      )
      .hide()
      .fadeIn(150);
    if (type === "success") $el.delay(4000).fadeOut(300);
  }
  function busy($btn, on) {
    $btn.prop("disabled", on).text(on ? "Saving…" : $btn.data("label"));
  }
  function statusBadge(status) {
    const cls =
      status === "Returned" ? "gray" : status === "Overdue" ? "bad" : "ok";
    return $("<span>")
      .addClass("badge " + cls)
      .text(status);
  }

  // ---------- render ----------
  function render(p) {
    document.title = p.fullName + " – Library";

    const $header = $("<div>")
      .addClass("card profile-head")
      .append(
        $("<div>")
          .addClass("avatar js-avatar")
          .css("background", `hsl(${hueOf(p.fullName)},55%,42%)`)
          .text(initials(p.fullName)),
        $("<div>").append(
          $("<h1>").addClass("profile-name js-name").text(p.fullName),
          $("<div>")
            .addClass("meta")
            .append(
              pill("✉️ " + p.email),
              $("<span>")
                .addClass("pill js-phone")
                .text("📞 " + (p.phoneNumber || "No phone")),
              pill(p.role === "Admin" ? "🛡️ Administrator" : "📖 Member"),
            ),
        ),
      );

    $box.hide().empty().append($header);

    if (p.role === "User") {
      $box.append(
        $("<div>")
          .addClass("stats")
          .append(
            stat(p.totalLoans, "Total loans"),
            stat(p.activeLoans, "Active now"),
            stat(p.overdueLoans, "Overdue"),
            stat(p.returnedLoans, "Returned"),
          ),
      );
    }

    const $grid = $("<div>")
      .addClass("profile-grid")
      .append(
        $("<div>")
          .addClass("card")
          .append($("<h3>").text("Account details"), buildInfoForm(p)),
        $("<div>")
          .addClass("card")
          .append($("<h3>").text("Change password"), buildPasswordForm()),
      );
    $box.append($grid);

    if (p.role === "User") $box.append(buildLoans(p.recentLoans));

    $box.fadeIn(250);
  }

  // ---------- recent loans ----------
  function buildLoans(loans) {
    const $card = $("<div>")
      .addClass("card")
      .append($("<h3>").text("Recent loans"));
    if (loans.length === 0) {
      return $card.append(
        $("<div>")
          .addClass("empty")
          .text("You haven't borrowed any books yet."),
        $("<a>").attr("href", "books.html").text("Browse books →"),
      );
    }
    $.each(loans, function (_, l) {
      const when = l.returnDate
        ? "Returned " + fmtDate(l.returnDate)
        : "Due " + fmtDate(l.dueDate);
      $card.append(
        $("<div>")
          .addClass("loan-row")
          .append(
            $("<div>").append(
              $("<div>").addClass("book-title").text(l.bookTitle),
              $("<div>")
                .addClass("muted")
                .text(`Borrowed ${fmtDate(l.loanDate)} · ${when}`),
            ),
            statusBadge(l.status),
          ),
      );
    });
    return $card.append(
      $("<a>").attr("href", "loans.html").text("View all loans →"),
    );
  }

  // ---------- edit details ----------
  function buildInfoForm(p) {
    const $name = $("<input>")
      .attr({ type: "text", required: true, minlength: 2, maxlength: 100 })
      .val(p.fullName);
    const $phone = $("<input>")
      .attr({ type: "tel", maxlength: 30, placeholder: "+962 7X XXX XXXX" })
      .val(p.phoneNumber || "");
    const $email = $("<input>")
      .attr({ type: "email", disabled: true })
      .val(p.email);
    const $out = $("<div>");
    const $btn = $("<button>")
      .attr("type", "submit")
      .data("label", "Save changes")
      .text("Save changes");

    const $form = $("<form>").append(
      field("Full name", $name),
      field("Email (cannot be changed)", $email),
      field("Phone number", $phone),
      $("<div>").css("margin-top", "14px").append($btn),
      $out,
    );

    $form.on("submit", function (e) {
      e.preventDefault();
      busy($btn, true);
      call("PUT", "/api/users/me", {
        fullName: $name.val().trim(),
        phoneNumber: $phone.val().trim() || null,
      })
        .done(function (u) {
          Session.save(Session.token(), u);
          renderNav("profile");
          $(".js-name").text(u.fullName);
          $(".js-avatar")
            .text(initials(u.fullName))
            .css("background", `hsl(${hueOf(u.fullName)},55%,42%)`);
          $(".js-phone").text("📞 " + ($phone.val().trim() || "No phone"));
          flash($out, "Profile updated.", "success");
        })
        .fail(function (xhr) {
          flash($out, errorText(xhr), "error");
        })
        .always(function () {
          busy($btn, false);
        });
    });
    return $form;
  }

  // ---------- change password ----------
  function strengthOf(pw) {
    let score = 0;
    if (pw.length >= 8) score++;
    if (/[A-Z]/.test(pw) && /[a-z]/.test(pw)) score++;
    if (/\d/.test(pw)) score++;
    if (/[^A-Za-z0-9]/.test(pw)) score++;
    return pw.length === 0 ? 0 : Math.max(score, 1);
  }

  function buildPasswordForm() {
    const $cur = $("<input>").attr({
      type: "password",
      required: true,
      autocomplete: "current-password",
    });
    const $new = $("<input>").attr({
      type: "password",
      required: true,
      minlength: 6,
      maxlength: 100,
      autocomplete: "new-password",
    });
    const $conf = $("<input>").attr({
      type: "password",
      required: true,
      autocomplete: "new-password",
    });
    const $bar = $("<span>");
    const $barLabel = $("<div>").addClass("muted");
    const $show = $("<input>")
      .attr("type", "checkbox")
      .css({ width: "auto", marginRight: "6px" });
    const $out = $("<div>");
    const $btn = $("<button>")
      .attr("type", "submit")
      .data("label", "Update password")
      .text("Update password");

    const levels = [
      { w: "0%", c: "#e5e7eb", t: "" },
      { w: "25%", c: "#dc2626", t: "Weak" },
      { w: "50%", c: "#d97706", t: "Fair" },
      { w: "75%", c: "#2563eb", t: "Good" },
      { w: "100%", c: "#16a34a", t: "Strong" },
    ];
    $new.on("input", function () {
      const lv = levels[strengthOf($new.val())];
      $bar.css({ width: lv.w, background: lv.c });
      $barLabel.text(lv.t);
    });

    $show.on("change", function () {
      const type = $show.is(":checked") ? "text" : "password";
      $cur.add($new).add($conf).attr("type", type);
    });

    const $form = $("<form>").append(
      field("Current password", $cur),
      field("New password", $new),
      $("<div>").addClass("strength").append($bar),
      $barLabel,
      field("Confirm new password", $conf),
      $("<label>")
        .css({ display: "flex", alignItems: "center", marginTop: "12px" })
        .append($show, "Show passwords"),
      $("<div>").css("margin-top", "14px").append($btn),
      $out,
    );

    $form.on("submit", function (e) {
      e.preventDefault();
      if ($new.val() !== $conf.val())
        return flash($out, "The new passwords do not match.", "error");
      if ($new.val() === $cur.val())
        return flash(
          $out,
          "The new password must be different from the current one.",
          "error",
        );

      busy($btn, true);
      call("PUT", "/api/users/me/password", {
        currentPassword: $cur.val(),
        newPassword: $new.val(),
      })
        .done(function () {
          $form[0].reset();
          $bar.css({ width: "0%" });
          $barLabel.text("");
          flash($out, "Password changed successfully.", "success");
        })
        .fail(function (xhr) {
          flash($out, errorText(xhr), "error");
        })
        .always(function () {
          busy($btn, false);
        });
    });
    return $form;
  }
});
