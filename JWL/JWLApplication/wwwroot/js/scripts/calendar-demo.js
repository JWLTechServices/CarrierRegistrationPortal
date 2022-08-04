

$(document).ready(function() {


    /* initialize the external events
    -----------------------------------------------------------------*/

    $('#external-events .ex-event').each(function() {

        // store data so the calendar knows to render an event upon drop
        $(this).data('event', {
            title: $.trim($(this).text()), // use the element's text as the event title
            stick: true, // maintain when user navigates (see docs on the renderEvent method)
            className: $(this).attr('data-class')
        });

        // make the event draggable using jQuery UI
        $(this).draggable({
            zIndex: 999,
            revert: true,      // will cause the event to go back to its
            revertDuration: 0  //  original position after the drag
        });

    });

    
    $('#calendar').fullCalendar({
        header: {
            left: 'prev,next today',
            center: 'title',
            right: 'month,basicWeek,basicDay'
        },
        defaultDate: '2017-10-12',
        navLinks: true, // can click day/week names to navigate views
        editable: true,
        droppable: true, // this allows things to be dropped onto the calendar
        eventLimit: true, // allow "more" link when too many events
        weekends: true,
        defaultView: 'agendaWeek',
        drop: function() {
            // is the "remove after drop" checkbox checked?
            if ($('#drop-remove').is(':checked')) {
                // if so, remove the element from the "Draggable Events" list
                $(this).remove();
            }
        }
    });
    
});
