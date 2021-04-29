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
		let param = {
			Url: "Common/AdminUserSiteList"
		};
		WebAPI.PostData(param).then(
			res => {
				if (isMountedRef.current) {
					if (res.data.errorCode === 0) {
						dispatch({
							type: defActionType.SITE_SET_LIST,
							siteList: res.data.data
						});
						dispatch({
							type: defActionType.FROM_CURRENCY_SET_LIST,
							siteList: res.data.data,
							fromSiteId:props.Datas.fromSiteId
						});
						dispatch({
							type: defActionType.TO_CURRENCY_SET_LIST,
							siteList: res.data.data,
							toSiteId:props.Datas.toSiteId
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
	}, []);

	useEffect(() => {
		if (!props.processing) {
			if (isMountedRef.current) {
				props.action.setProcessing(true);
			}
			let ReasonListParam = {
				Url: "TransferManage/GetTransferAccountReason"
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
				Url: "TransferManage/GetSysBankList"
			};
			WebAPI.PostData(GetBankCodeListParam).then(
				res => {
					if (isMountedRef.current) {
						if (res.data.errorCode === 0) {
							dispatch({
								type: defActionType.BANK_CODE_SET_LIST,
								bankCodeList: res.data.data
							});
							dispatch({
								type: defActionType.FROM_BANK_SET_LIST,
								fromCurrencyId:props.Datas.fromCurrencyId
							});
							dispatch({
								type: defActionType.TO_BANK_SET_LIST,
								toCurrencyId:props.Datas.toCurrencyId
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
				Url: "TransferManage/GetPayList"
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
		initialState(props.Datas ,props.IsAdd)
	);

	const ClickSubmit = () => {
		props.action.SetTransferInfo(state);
	};

	const EditMainValue = (value, dataColumn) => {
		dispatch({ 
			type: defActionType.MAIN_SET_VALUE, 
			value, 
			dataColumn });
	};

	const EditTransferTypeValue = (value, dataColumn) => {
		dispatch({ 
			type: defActionType.MAIN_SET_TRANSFERTYPE_VALUE, 
			value, 
			dataColumn });
	};

	const EditTransactionTypeValue = (value, dataColumn) => {
		dispatch({ 
			type: defActionType.MAIN_SET_TRANSACTIONTYPE_VALUE, 
			value, 
			dataColumn });
	};

	const EditSiteSelectBox = (value, dataColumn) => {
		if(dataColumn == "fromSiteId")
		{
			dispatch({
				type: defActionType.MAIN_SET_FROM_SITEID_VALUE,
				value,
				dataColumn
			});
		}
		else
		{
			dispatch({
				type: defActionType.MAIN_SET_TO_SITEID_VALUE,
				value,
				dataColumn
			});
		}
	};

	const EditCurrencySelectBox = (value, dataColumn) => {
		if(dataColumn=="fromCurrencyId")
		{
			dispatch({
				type: defActionType.MAIN_SET_FROM_CURRENCY_VALUE,
				value,
				dataColumn
			});
		}else{
			dispatch({
				type: defActionType.MAIN_SET_TO_CURRENCY_VALUE,
				value,
				dataColumn
			});
		}
	};

	const EditChannelSelectBox = (value, dataColumn) => {
		if(dataColumn=="fromChannel")
		{
			dispatch({
				type: defActionType.MAIN_SET_FROM_CHANNEL_VALUE,
				value,
				dataColumn
			});
		}
		else
		{
			dispatch({
				type: defActionType.MAIN_SET_TO_CHANNEL_VALUE,
				value,
				dataColumn
			});
		}
	};

	return (
		<Panel
			dispatch={dispatch}
			state={state}
			action={{
				ClickSubmit,
				EditMainValue,
				EditTransferTypeValue,
				EditTransactionTypeValue,
				EditSiteSelectBox,
				EditCurrencySelectBox,
				EditChannelSelectBox,
				ClickBack: props.action.ClickBack,
			}}
			isAdd={props.IsAdd}
			processing={props.processing}
		/>
	);
};

