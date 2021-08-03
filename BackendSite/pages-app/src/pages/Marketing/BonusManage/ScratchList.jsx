import React, { useReducer, useEffect, useRef ,useState} from "react";
import moment from "moment";
import { reducer, defActionType, initialState } from "./ScratchList.reducer";
import { initFilterType } from "./defCols";
import SelectBox from "./Component/SelectBox";
import WebAPI from "../../../module/WebAPI";
import qs from "qs";
import Panel from "./ScratchList.content";
import PopBoxContentforGen from "./Component/PopBoxContentforGen";

const dateFormat = createTime => {
	return moment(createTime).format("l LTS");
};

const lastUpdateUserFormat = userName => {
	return userName ?? "-";
};
const JoinerFormat = userName => {
	return userName ?? "Unused";
};
const titleData = [
	{ dataColumn: "scratchCardCode", columnTitle: "ScratchCardCode" },
	{
		dataColumn: "userName",
		columnTitle: "Joiner",
		body: { format: lastUpdateUserFormat }
	},
	{
		dataColumn: "createTime",
		columnTitle: "CreateTime(+08:00)",
		body: { format: dateFormat }
	},
	{
		dataColumn: "updateTime",
		columnTitle: "UpdateTime(+08:00)",
		body: { format: dateFormat }
	},
	{
		dataColumn: "lastUpdateUser",
		columnTitle: "Creator",
		body: { format: lastUpdateUserFormat }
	}
];

export default props => {
const [FilterStatus, setFilterStatus] = useState(0);
const FilterOnchange = (value, dataColumn)=>{
	setFilterStatus(value);
}
const Result_ExportToExcel = () => {

	if(props.Datas.length !== 0 )
	{
		let param = {
			Url: "BonusManage/GetScratchCardListExport",
			Data: {
				SpecialCode: props.Datas[0].specialCode,
				FilterType:FilterStatus,
			},
		};
		WebAPI.DownloadData(param);
	}
  };
	const isMountedRef = useRef(null);
	useEffect(() => {
		isMountedRef.current = true;
		return () => (isMountedRef.current = false);
	});
	const [state, dispatch] = useReducer(
		reducer,
		initialState( props.Datas)
	);

	
    let headerRows = [];
	titleData.forEach((e, i) => {
		headerRows.push(<th key={`title-${i}`}>{e.columnTitle}</th>);
	});
	//console.log('FilterStatus = '+FilterStatus);
	let data = props.Datas;
	if(FilterStatus == 1)
		 data = props.Datas.filter(x=>x.custId == 0);
	else if(FilterStatus == 2)
	    data = props.Datas.filter(x=>x.custId != 0);

	//	console.log(data);
	// let bodyRows = props.Datas.map((_data, d_idx) => {
	// 	let arr = titleData.map((element, t_idx) => {
	// 		let body_content = null;
	// 		body_content =
	// 			element.body && element.body.format
	// 				? element.body.format(_data[element.dataColumn])
	// 				: _data[element.dataColumn];
	// 		  return <td key={`td-${t_idx}`}>{body_content}</td>;
	// 	});

	// 	return <tr key={`body-${d_idx}`}>{arr}</tr>;
	// });

	let bodyRows = data.map((_data, d_idx) => {
		let arr = titleData.map((element, t_idx) => {
			let body_content = null;
			body_content =
				element.body && element.body.format
					? element.body.format(_data[element.dataColumn])
					: _data[element.dataColumn];
			  return <td key={`td-${t_idx}`}>{body_content}</td>;
		});

		return <tr key={`body-${d_idx}`}>{arr}</tr>;
	});

	return (
		<div className="container-fluid">
			<h4 className="page-name">ScratchCard List</h4>	
			<div className="table">
			<div className="filter">
					<div className="row-flex">
				<button
					className="btn btn-add"
					onClick={props.action.ClickGen}>
					Generar ScratchCard
				</button>
				<button
					className="btn btn-approved"
					onClick={Result_ExportToExcel}>
					Export Excel
				</button>
				
						<SelectBox
								style={{ marginleft: "20px" }}
								Title="Status"
								Options={initFilterType}
								HandleChange={FilterOnchange}
								dataColumn="Status"
								SelectedValue={FilterStatus}
						/>
					</div>
				</div>
				<table className="table table-striped">
					<thead>
						<tr>{headerRows}</tr>
					</thead>
					<tbody>{bodyRows}</tbody>
				</table>
			</div>
			{
				
				props.IsOpenGen ?
					<PopBoxContentforGen
						Action={props.action}
					/>
					: null
			}
			<Panel
				dispatch={dispatch}
				state={state} 
				action={{
					ClickBack: props.action.ClickBack
				}}

				processing={props.processing}
			/>
			
		</div>
		
	);

	// useEffect(() => {
	// 	if (!props.IsAdd) {
	// 		GetPromoCategories(true, -1, props.Datas.walletType);
	// 	}
	// }, [props.Datas.walletType, props.IsAdd]);

	

	

	const GetPromoCategories = (isInit, walletType, value) => {
		if (walletType !== value) {
			let param = {
				Url: "BonusManage/GetPromoCategories",
				Data: qs.stringify({
					WalletType: value
				})
			};
			WebAPI.PostData(param).then(res => {
				if (isMountedRef.current) {
					if (res.data.errorCode === 0) {
						dispatch({
							type: defActionType.GET_PROMO_CATEGORIES,
							value,
							productCategory: res.data.data,
							isInit: isInit
						});
					} else {
						alert(res.data.message ?? "failed to get data.");
					}
				}
			});
		}
	};

	return (
		<>
			<Panel
				dispatch={dispatch}
				state={state}
				action={{
					ClickBack: props.action.ClickBack
				}}

				processing={props.processing}
			/>
		</>
	);
};

