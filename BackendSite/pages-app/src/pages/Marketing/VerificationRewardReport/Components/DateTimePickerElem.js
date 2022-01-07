import React from "react";
import DateTimePicker from "react-datetime";
import moment from "moment";

export default function DateTimePickerElem ({ title, datetime, timeFormat, dataColumn, onChange = f => f }) {
	return (
		<div className="input-item">
			<span className="label">{title}</span>
			<DateTimePicker
                defaultValue={datetime}
                value={datetime}
                timeFormat={timeFormat}
                onChange={(e) => {
                    try {
                        onChange(moment(e).format("l"), dataColumn);
                    } catch (ex) { }
                }} />
		</div>
	);
};