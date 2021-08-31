import React from "react"

export default function ActionButtons ({ ClickEdit }) {
	return (
		<div className="button-action">
			<button className="btn btn-edit" title="Only can edit time column." onClick={ClickEdit}>
				<i className="icon-edit"></i>
			</button>
		</div>
	);
};