import React from "react";
import DateTimePicker from "react-datetime";
import moment from "moment";

const filterdateformat = "YYYY-MM-DD";

export default function DateTimePickerElem ({ title, value, timeFormat, dataColumn, onChange = f => f }) {
	return (
		<div className="input-item">
			<span className="label">{title}</span>
			<DateTimePicker 
                defaultValue={moment(value).format(filterdateformat)}
                timeFormat={timeFormat}
                onBlur={(e) => {
                    try {
                        onChange(moment(e).format(filterdateformat), dataColumn);
                    } catch (ex) { }
                }} />
		</div>
	);
};