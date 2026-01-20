document.onreadystatechange = function () {
    if (document.readyState === "complete"){
        afadmin_getProperties();

	    const disableTextSelect = (elements) => {
		    elements.forEach(element => {
			    element.style.userSelect = 'none';
		    });
	    };
	    disableTextSelect(document.querySelectorAll('.noSelect')); 
    }	
};