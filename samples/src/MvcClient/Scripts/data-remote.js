$('form').live('submit', function (e) {
    var form = $(this);
    if (form.attr('data-remote')) {
        var method = form.attr('method');
        var url = form.attr('action');
        var data = form.serializeArray();
        $.ajax({
            url: url,
            type: method || 'GET',
            data: data,
            contentType: 'application/json',
            success: function (data, status, xhr) {
                form.trigger('ajax:success', [data, status, xhr]);
            },
            complete: function (xhr, status) {
                form.trigger('ajax:complete', [xhr, status]);
            },
            error: function (xhr, status, error) {
                form.trigger('ajax:error', [xhr, status, error]);
            }
        });
        return false;
    }
});