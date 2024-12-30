//remove the default 'Search' text for all DataTable search boxes
$.extend(true, $.fn.dataTable.defaults, {
    language: {
        search: ""
    }
});


$(function () {
    console.log("ready!");
    $('[type=search]').each(function () {
        $(this).attr("placeholder", "filter");
        $(this).before('<i class="bi-search" style="font-size: 1.2rem; color: silver;"></i>');
    });
});
//custom format of Search boxes
