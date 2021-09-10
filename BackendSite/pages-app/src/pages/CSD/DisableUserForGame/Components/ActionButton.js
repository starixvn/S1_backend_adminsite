import React from "react"

export default function ActionButtons ({ ClickEdit, ClickDelete}) {
	return (
		<div className="button-action">
			<button className="btn btn-edit" onClick={ClickEdit}>
				<i className="icon-edit"></i>
			</button>
			<button className="btn btn-icon-delete" onClick={ClickDelete}>
				<i className="icon-delete"></i>
			</button>
		</div>
	);
};