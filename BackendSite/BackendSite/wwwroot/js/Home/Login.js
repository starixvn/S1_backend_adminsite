$(function () {
    $("#frmLogin").submit(function (event) {
        event.preventDefault();
        var obj;
        obj = document.getElementById('txtID');
        if (obj.value == '') {
            obj.focus();
            alert(i18n.errlogin_enter_un);
            return;
        }

        obj = document.getElementById('txtPW');
        if (obj.value == '') {
            obj.focus();
            alert(i18n.errlogin_enter_pw);
            return;
        }

        var data = {txtID:$("#txtID").val(),txtPW:$("#txtPW").val()};
        axios.post('/api/Home/Signin',data)
        .then(function (response) {
            let result = response.data;
            if (result.errorCode == 0) {
                window.top.location.replace(result.message);
            } else {
                alert(result.message);
            }
        }).catch(function (error) {
            alert(error);
        });
    });
});