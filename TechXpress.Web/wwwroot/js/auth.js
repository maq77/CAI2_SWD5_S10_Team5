$(function () {
    $("#registerBtn").on("click", function () {
        var userData = {
            FirstName: $("#FirstName").val(),
            LastName: $("#LastName").val(),
            Email: $("#Email").val(),
            Password: $("#Password").val(),
            ConfirmPassword: $("#ConfirmPassword").val()
        };

        $.ajax({
            url: "/Account/Register",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(userData),
            success: function (response) {
                console.log("Register Response:", response); // Debugging output
                if (response.redirectUrl) {
                    window.location.href = response.redirectUrl;
                } else {
                    $("#register-message").removeClass("d-none alert-success")
                        .addClass("alert-danger")
                        .text("Registration successful, but no redirect URL found.");
                }
            },
            error: function (xhr) {
                console.log("Register Error:", xhr.responseText);
                $("#register-message").removeClass("d-none alert-success")
                    .addClass("alert-danger")
                    .text(xhr.responseJSON?.message || "An error occurred.");
            }
        });
    });

    $("#loginBtn").on("click", function () {
        //alert("Clicked!");
        $("#loginBtn").prop("disabled", true).text("Logging in...");

        var loginData = {
            Email: $("#Email").val(),
            Password: $("#Password").val(),
            RememberMe: $("#RememberMe").is(":checked"),
            returnUrl: $("#returnUrl").val()
        };

        $.ajax({
            url: "/Account/Login",
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(loginData),
            success: function (response) {
                if (response.redirectUrl) {
                    window.location.href = response.redirectUrl;
                } else {
                    $("#login-message").removeClass("d-none alert-success")
                        .addClass("alert-danger")
                        .text("Login successful, but no redirect URL found.");
                    $("#loginBtn").prop("disabled", false).text("Login");
                }
            },
            error: function (xhr) {
                $("#login-message").removeClass("d-none alert-success")
                    .addClass("alert-danger")
                    .text(xhr.responseJSON?.message || "Invalid credentials.");
                $("#loginBtn").prop("disabled", false).text("Login");
            }
        });
    });

    $("#logoutBtn").on("click", function () {
        // Show confirmation modal instead of alert
        $('#logoutModal').modal('show');
    });

    /*$("#logoutBtn").on("click", function () {
        // Optional: Show loading state
        $(this).prop('disabled', true).text('Logging out...');

        $.ajax({
            url: "/Account/Logout",
            type: "POST",
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (response) {
                console.log("Logout Response:", response);
                if (response.success) {
                    localStorage.clear();
                    sessionStorage.clear();

                    window.location.href = response.redirectUrl || "/Account/Login";
                } else {
                    alert("Logout failed: " + (response.message || "Unknown error"));
                    $("#logoutBtn").prop('disabled', false).text('Logout');
                }
            },
            error: function (xhr, status, error) {
                console.error("Logout Error:", {
                    status: xhr.status,
                    statusText: xhr.statusText,
                    responseText: xhr.responseText,
                    error: error
                });

                let errorMessage = "An error occurred during logout.";
                if (xhr.status === 403) {
                    errorMessage = "Session expired. Please refresh the page.";
                } else if (xhr.status === 500) {
                    errorMessage = "Server error. Please try again.";
                }

                alert(errorMessage);

                $("#logoutBtn").prop('disabled', false).text('Logout');
            },
            timeout: 10000 // 10 sec timeout //new feature
        });
    });*/

    $("#confirmLogoutBtn").on("click", function () {
        // Hide the modal
        $('#logoutModal').modal('hide');

        // Show loading state on original logout button
        $("#logoutBtn").prop('disabled', true).text('Logging out...');

        $.ajax({
            url: "/Account/Logout",
            type: "POST",
            headers: {
                'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
            },
            success: function (response) {
                console.log("Logout Response:", response);
                if (response.success) {
                    localStorage.clear();
                    sessionStorage.clear();
                    window.location.href = response.redirectUrl || "/Account/Login";
                } else {
                    alert("Logout failed: " + (response.message || "Unknown error"));
                    $("#logoutBtn").prop('disabled', false).text('Logout');
                }
            },
            error: function (xhr, status, error) {
                console.error("Logout Error:", {
                    status: xhr.status,
                    statusText: xhr.statusText,
                    responseText: xhr.responseText,
                    error: error
                });
                let errorMessage = "An error occurred during logout.";
                if (xhr.status === 403) {
                    errorMessage = "Session expired. Please refresh the page.";
                } else if (xhr.status === 500) {
                    errorMessage = "Server error. Please try again.";
                }
                alert(errorMessage);
                $("#logoutBtn").prop('disabled', false).text('Logout');
            },
            timeout: 10000 // 10 sec timeout
        });
    });

    $('#resend-confirmation-btn').on("click", function (e) {
        e.preventDefault();
        $('#resend-message').text("Sending...").css('color', 'gray');

        $.ajax({
            url: '/Account/ResendConfirmationEmail',
            method: 'POST',
            success: function (response) {
                showAnimatedMessage(response.message, response.success);
            },
            error: function () {
                showAnimatedMessage("An error occurred. Please try again.", false);
            }
        });
    });
    function showAnimatedMessage(message, isSuccess) {
        const $msg = $("#confirmation-message");
        $msg.removeClass("d-none alert-success alert-danger show fade");

        $msg
            .addClass(isSuccess ? "alert-success" : "alert-danger")
            .text(message)
            .addClass("show fade")
            .hide()
            .fadeIn(500);

        setTimeout(() => {
            $msg.fadeOut(500, () => {
                $msg.addClass("d-none");
            });
        }, 4000); // Message visible for 4 seconds
    }
});
