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
        $.ajax({
            url: "/Account/Logout",
            type: "POST",
            success: function (response) {
                console.log("Logout Response:", response);
                if (response.success) {
                    window.location.href = response.redirectUrl || "/"; // Redirect to login page
                } else {
                    alert("Logout failed.");
                }
            },
            error: function (xhr) {
                console.log("Logout Error:", xhr.responseText);
                alert("An error occurred during logout.");
            }
        });
    });
});
