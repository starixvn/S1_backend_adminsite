import React, { useState, useEffect, useRef } from "react";
import ReactDOM from "react-dom";
import Result from "./result";
import WebAPI from "../../../module/WebAPI";
import qs from "qs";

const Panel = () => {
	const [stateData, setData] = useState([]);
	const [processing, setProcessing] = useState(false);
	const isMountedRef = useRef(null);
	useEffect(() => {
		isMountedRef.current = true;
		return () => (isMountedRef.current = false);
	});

	useEffect(() => {
		let param = {
			Url: "BonusInfo/GetPendingBonusList"
		};
		WebAPI.PostData(param).then(res => {
			if (res.data.errorCode === 0 && isMountedRef.current) {
				setData(res.data.data);
			}
		});
	}, []);

	const GetPendingBonus = () => {
		let param = {
			Url: "BonusInfo/GetPendingBonusList"
		};
		WebAPI.PostData(param).then(
			res => {
				if (isMountedRef.current) {
					if (res.data.errorCode === 0) {
						setData(res.data.data);
					}else{
						alert(res.data.message);
					}
					setProcessing(false);
				}
			},
			err => {
				if (isMountedRef.current) {
					alert('API Server error.');
					setProcessing(false);
				}
			}
		);
	};

	const ClickCancel = idx => {
		if (!processing) {
			if (window.confirm("cancel this user's bonus?")) {
				setProcessing(true);
				let param = {
					Url: "PendingFund/BonusCancel",
					Data: qs.stringify({ BonusId: stateData[idx].bonusId })
				};
				WebAPI.PostData(param).then(
					res => {
						if (isMountedRef.current) {
							if (res.data.errorCode === 0) {
								GetPendingBonus();
							}else{
								alert(res.data.message);
								setProcessing(false);
							}
						}
					},
					err => {
						if (isMountedRef.current) {
							alert('API Server error.');
							setProcessing(false);
						}
					}
				);
			}
		}
	};

	const ClickTransfer = idx => {
		if (!processing) {
			if (window.confirm("transfer bonus to this user?"))
				setProcessing(true);
			let param = {
				Url: "PendingFund/BonusTransfer",
				Data: qs.stringify({ CustId: stateData[idx].custId ,TpId: stateData[idx].tpId})
			};
			WebAPI.PostData(param).then(
				res => {
					if (isMountedRef.current) {
						if (res.data.errorCode === 0) {
							GetPendingBonus();
						}else{
							alert(res.data.message);
							setProcessing(false);
						}
					}
				},
				err => {
					if (isMountedRef.current) {
						alert('API Server error.');
						setProcessing(false);
					}
				}
			);
		}
	};

	const ClickTransferAll = () => {
		if (!processing) {
			if (window.confirm("execute transfer all bonus?")) {
				setProcessing(true);
				let param = {
					Url: "PendingFund/BonusTransferAll"
				};
				WebAPI.PostData(param).then(
					res => {
						if (isMountedRef.current) {
							if (res.data.errorCode === 0) {
								GetPendingBonus();
							}else{
								alert(res.data.message);
								setProcessing(false);
							}
						}
					},
					err => {
						if (isMountedRef.current) {
							alert('API Server error.');
							setProcessing(false);
						}
					}
				);
			}
		}
	};

	return (
		<div className="container-fluid">
			<h4 className="page-name">Pending Bonus</h4>
			<button
				className="btn btn-search"
				onClick={ClickTransferAll}
				style={{ marginBottom: "20px" }}
				disabled={processing}>
				Transfer All
			</button>
			<Result
				Datas={stateData}
				ClickCancel={ClickCancel}
				ClickTransfer={ClickTransfer}
				processing={processing}
			/>
		</div>
	);
};

export default Panel;

if (process.env.NODE_ENV === "production") {
	ReactDOM.render(<Panel />, document.getElementById("content"));
}
