import React from "react";

const initBonusAmountType = promoType => {
	return Number(promoType) === 3
		? [{ value: 2, displayText: "Amount" }]
		: [
			{ value: 1, displayText: "Percentage" },
			{ value: 2, displayText: "Amount" }
		];
};

export default ({ dispatch, state, action, IsAdd, processing }) => {
	
	



	return (
		<div className="container-fluid" id="Panel">
			<div className="filter noBGcolor"></div>
			
			
			<div className="filter float-btn-group">
				<button
					className="btn btn-back"
					onClick={action.ClickBack}
					disabled={processing}>
					Back
				</button>
			</div>
		</div>
	);
};


