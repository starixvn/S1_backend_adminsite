import React, { useReducer, useEffect, useRef } from "react";
import { reducer, defActionType, initialState } from "./Edit.reducer";
import Panel from "./Edit.content";
import WebAPI from "../../../module/WebAPI";
import {api} from "../../../module/WebAPI";

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

	useEffect(() => {
		if (!props.processing) {
			if (isMountedRef.current) {
				props.action.setProcessing(true);
			}
			let GetPayListParam = {
				Url: "TransferManage/GetTransferAccountList"
			};
			WebAPI.PostData(GetPayListParam).then(
				res => {
					if (isMountedRef.current) {
						if (res.data.errorCode === 0) {
							dispatch({
								type: defActionType.TRANSFER_ACCOUNT_SET_LIST,
								transferAccountList: res.data.data
							});
							dispatch({
								type: defActionType.FROM_ACCOUNT_SET_LIST,
								fromChannel:props.Datas.fromChannel,
								fromSiteId:props.Datas.fromSiteId,
								fromCurrencyId:props.Datas.fromCurrencyId,
								fromSysId:props.Datas.fromSysId
							});
							dispatch({
								type: defActionType.TO_ACCOUNT_SET_LIST,
								toChannel:props.Datas.toChannel,
								toSiteId:props.Datas.toSiteId,
								toCurrencyId:props.Datas.toCurrencyId,
								toSysId:props.Datas.toSysId
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

	const EditBankCodeSelectBox = (value, dataColumn) => {
		if(dataColumn=="fromSysId")
		{
			dispatch({
				type: defActionType.MAIN_SET_FROM_BANK_VALUE,
				value,
				dataColumn
			});
		}else{
			dispatch({
				type: defActionType.MAIN_SET_TO_BANK_VALUE,
				value,
				dataColumn
			});
		}
	};

	const EditAccountSelectBox = (value, dataColumn) => {
		console.log(value);
		console.log(dataColumn);
		if(dataColumn=="fromAccount")
		{
			dispatch({
				type: defActionType.MAIN_SET_FROM_ACCOUNT_VALUE,
				value,
				dataColumn
			});
		}else{
			dispatch({
				type: defActionType.MAIN_SET_TO_ACCOUNT_VALUE,
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

	const UploadReceipt = (value, dataColumn) => {
	
		let settings = { headers: { "content-type": "multipart/form-data" } };
		let file = document.getElementById("UploadReceipt").files[0];
		var formData = new FormData();
		formData.append("UploadReceipt", file, file.name);
	
		api.post("/api/TransferManage/ReviewTransferReceiptPic", formData, settings)
		.then(function (response) {
		if (response.data) {
			let uploadReceiptPic =response.data;
			    dispatch({ 
					type: defActionType.SET_UPLOAD_RECEIPT, 
					uploadReceiptPic,
					dataColumn 
				});
				} else {
				  alert("sent error");
				}
			  })
			  .catch(function (error) {
				alert(error);
			  });

			dispatch({ 
				type: defActionType.MAIN_SET_VALUE, 
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
				EditTransferTypeValue,
				EditTransactionTypeValue,
				EditSiteSelectBox,
				EditCurrencySelectBox,
				EditChannelSelectBox,
				EditBankCodeSelectBox,
				EditAccountSelectBox,
				UploadReceipt,
				ClickBack: props.action.ClickBack,
			}}
			isAdd={props.IsAdd}
			processing={props.processing}
		/>
	);
};

