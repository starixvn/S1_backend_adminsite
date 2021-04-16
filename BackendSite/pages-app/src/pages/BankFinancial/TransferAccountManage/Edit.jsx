import React, { useReducer, useEffect, useRef } from "react";
import { reducer, defActionType, initialState } from "./Edit.reducer";
import Panel from "./Edit.content";
import WebAPI from "../../../module/WebAPI";

export default props => {
	const isMountedRef = useRef(null);
	useEffect(() => {
		isMountedRef.current = true;
		return () => (isMountedRef.current = false);
	});

	useEffect(() => {
		if (!props.processing) {
			if (isMountedRef.current) {
				props.action.setProcessing(true);
			}
			let ReasonListParam = {
				Url: "TransferAccountManage/GetTransferAccountReason"
			};
			WebAPI.PostData(ReasonListParam).then(
				res => {
					if (isMountedRef.current) {
						if (res.data.errorCode === 0) {
							
							dispatch({
								type: defActionType.REASON_SET_LIST,
								reasonList: res.data.data
							});
						} else {
							alert(res.data.message ?? "failed to get data.");
						}
						props.action.setProcessing(false);
					}
				},
				err => {
					if (isMountedRef.current) {
						alert("API Server Error.");
						props.action.setProcessing(false);
					}
				}
			);
		}
	}, []);

	useEffect(() => {
		if (!props.processing) {
			if (isMountedRef.current) {
				props.action.setProcessing(true);
			}
			let GetBankCodeListParam = {
				Url: "TransferAccountManage/GetSysBankList"
			};
			WebAPI.PostData(GetBankCodeListParam).then(
				res => {
					if (isMountedRef.current) {
						if (res.data.errorCode === 0) {
							dispatch({
								type: defActionType.BANK_CODE_SET_LIST,
								bankCodeList: res.data.data
							});
						} else {
							alert(res.data.message ?? "failed to get data.");
						}
						props.action.setProcessing(false);
					}
				},
				err => {
					if (isMountedRef.current) {
						alert("API Server Error.");
						props.action.setProcessing(false);
					}
				}
			);
		}
	}, []);

	useEffect(() => {
		if (!props.processing) {
			if (isMountedRef.current) {
				props.action.setProcessing(true);
			}
			let GetPayListParam = {
				Url: "TransferAccountManage/GetPayList"
			};
			WebAPI.PostData(GetPayListParam).then(
				res => {
					if (isMountedRef.current) {
						if (res.data.errorCode === 0) {
							dispatch({
								type: defActionType.PAY_SET_LIST,
								payList: res.data.data
							});
						} else {
							alert(res.data.message ?? "failed to get data.");
						}
						props.action.setProcessing(false);
					}
				},
				err => {
					if (isMountedRef.current) {
						alert("API Server Error.");
						props.action.setProcessing(false);
					}
				}
			);
		}
	}, []);

	const [state, dispatch] = useReducer(
		reducer,
		initialState(props.Datas ,props.SetCurrencyList ,props.IsAdd)
	);

	const ClickSubmit = () => {
		props.action.SetTransferAccount(state);
	};

	const EditMainValue = (value, dataColumn) => {
		dispatch({ 
			type: defActionType.MAIN_SET_VALUE, 
			value, 
			dataColumn });
	};

	const EditSiteIdSelectBox = (value, dataColumn) => {
		console.log("aaa",value);
		dispatch({
			type: defActionType.MAIN_SET_SITEID_VALUE,
			value,
			dataColumn
		});
	};

	const EditCurrencySelectBox = (value, dataColumn) => {
		dispatch({
			type: defActionType.MAIN_SET_CURRENCY_VALUE,
			value,
			dataColumn
		});
	};

	const EditChannelSelectBox = (value, dataColumn) => {
		dispatch({
			type: defActionType.MAIN_SET_CHANNEL_VALUE,
			value,
			dataColumn
		});
	};

	const EditPurposeCheckBox = (value, dataColumn) => {
		dispatch({
			type: defActionType.MAIN_SET_PURPOSE_CHECKBOX,
			value,
			dataColumn
		});
	};

	return (
		<Panel
			dispatch={dispatch}
			state={state}
			action={{
				ClickSubmit,
				EditMainValue,
				EditSiteIdSelectBox,
				EditPurposeCheckBox,
				EditCurrencySelectBox,
				EditChannelSelectBox,
				ClickBack: props.action.ClickBack,
			}}
			isAdd={props.IsAdd}
			processing={props.processing}
		/>
	);
};

