$(function () {
    $(".new-contrib").modal();
});

$(".deposit-button").on('click', function () {
    const contribId = $(this).data('contribid');
    $('[name="contributorId"]').val(contribId);

    const tr = $(this).closest('tr');
    const name = tr.find('td:eq(1)').text();
    $("#deposit-name").text(name);

    $("#deposit").modal();
});

$("#search").on('keyup', function () {
    const text = $(this).val();
    $("table tr;gt(0)").each(function () {
        const tr = $(this);
        const name = tr.find('td:eq(1)').text();
        if (name.toLowerCase().indexOf(text.toLowerCase()) !== -1) {
            tr.show();
        } else {
            tr.hide
        }
    });
});

$("#clear").on('click', function () {
    $("#search").val('');
    $("tr").show();
});

$(".edit-contrib").on('click', function () {
    const id = $(this).data('id');
    const name = $(this).data('name');
    const phoneNumber = $(this).data('phoneNumber');
    const alwaysInclude = $(this)

    const form = $(".new-contrib form");
    form.find("#edit-id").remove();
    const hidden = (`<input type = 'hidden' id='edit-id' name = 'id'value = '${id} />`);
    form.append(hidden);

    $("#initialDepositDiv").hide();

    $("#name").val(name);
    $("#contributor_phone_number").val(phoneNumber);
    $("#contributor_always_include").prop('checked', alwaysInclude === "True");
    $(".new-contrib").modal();
    form.attr('action', '/Home/edit');
})