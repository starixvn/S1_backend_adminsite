import React from "react";
import DateTimePicker from "react-datetime";

export default props => {
	return (
		<div className="input-item">
			<span className="label">{props.columnTitle}</span>
			<DateTimePicker {...props.dateTimePickerProps} />
		</div>
	);
};
