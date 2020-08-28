import React, { useReducer, useEffect, useRef } from "react";
import RadioBox from "./Component/RadioBox";
import { reducer, defActionType, initialState } from "./edit.reducer";
import WebAPI from "../../../module/WebAPI";
import qs from "qs";
import Content from "./edit.content";

export default ({
	datas,
	siteList,
	isAdd,
	ClickBack,
	ClickSubmit,
	processing
}) => {
	const isMountedRef = useRef(null);
	useEffect(() => {
		isMountedRef.current = true;
		return () => (isMountedRef.current = false);
	});
	const [state, dispatch] = useReducer(
		reducer,
		initialState(isAdd, datas, siteList)
	);

	let _options_site = siteList.map(s => {
		return { value: s.siteId, displayText: s.siteName };
	});

	const SelectSite = val => {
		let site = siteList.find(s => s.siteId === val);
		dispatch({
			type: defActionType.SET_SITE,
			siteId: val,
			langList: site.langList
		});
	};

	const EditValueByLang = (langId, value, column) => {
		dispatch({
			type: defActionType.SET_VALUE_BY_LANG,
			langId: langId,
			col: column,
			value: value
		});
	};

	const EditValue = (value, column) => {
		dispatch({
			type: defActionType.SET_VALUE,
			col: column,
			value: value
		});
	};

	const onSubmit = () => {
		ClickSubmit(state);
	};

	return (
		<>
			<div className="filter noBGcolor">
				<RadioBox
					Title="Available Sites"
					Options={_options_site}
					OnChange={SelectSite}
					dataColumn="availableSites"
					Selected={state.siteId}
					disabled={!isAdd}
				/>
			</div>
			<div className="container-fluid" id="Panel">
				<Content
					state={state}
					EditValueByLang={EditValueByLang}
					EditValue={EditValue}
					IsAdd={isAdd}
				/>
				<div className="filter float-btn-group">
					<button
						className="btn btn-back"
						onClick={ClickBack}
						disabled={processing}>
						Back
					</button>
					<button
						className="btn btn-add"
						onClick={onSubmit}
						disabled={processing}>
						Submit
					</button>
				</div>
			</div>
		</>
	);
};
