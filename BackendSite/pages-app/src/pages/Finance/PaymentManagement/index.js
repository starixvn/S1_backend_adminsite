import React from "react";
import ReactDOM from 'react-dom';

if(process.env.NODE_ENV!=='production'){
    window.SiteList = {1:"Nova88", 2:"Bwing", 3:"123Goal"};
}

const App = ()=>{
    const SelectSite = (v)=>{
        console.log('radio-box select:', v);
    }

    let sites = Object.keys(window.SiteList).map(k=>{
        return (
            <RadioBox 
                key={`rd-sites-${k}`}
                DataColumn="Site"
                Id={k}
                DisplayText={window.SiteList[k]}
                Value={k}
                SelectSite={SelectSite}
            />
        );
    });

    return(
        <div className="container-fluid">
            <h4 className="page-name">PaymentManagement</h4>
            <button className="btn btn-add">Add Payment</button>
            <div className="form-group">
                <label className="col-form-label d-block">SiteName</label>
                {sites}
            </div>
            <OuterPanel 
                Content={<OuterPanel Content={<PaymentList />} />} 
            />
        </div>
    )
}

const RadioBox = (props)=>{
    let id = `${props.DataColumn}-${props.Id}`;
    return(
        <div className="custom-control custom-radio custom-control-inline" >
            <input type="radio" id={id} name={props.DataColumn} className="custom-control-input" value={props.Value} onChange={props.SelectSite.bind(null, props.Value)}/>
            <label className="custom-control-label" htmlFor={id}>{props.DisplayText}</label>
        </div>
    )
}

const OuterPanel = (props)=>{
    return(
        <div className="panel mt-20">
            <a className="panel-title" data-toggle="collapse" href="#collapseExample-2" role="button" aria-expanded="true" aria-controls="collapseExample">
                MYR
                <button className="btn btn-add">Add Bank Card</button>
            </a>
            <div className="panel-body">
                {props.Content}
            </div>
        </div>
    )
}

const columnDefs = ()=>{
    return [
        {
            dataColumn:'MinAmount', columnTitle:'Min Amount',
            header:{
                attr:{
                    className:'text-right'
                }
            },
            body:{
                attr:{
                    className:'text-right'
                }
            }
        },
        {
            dataColumn:'MinAmount', columnTitle:'Min Amount',
            header:{
                attr:{
                    className:'text-right'
                }
            },
            body:{
                attr:{
                    className:'text-right'
                }
            }
        }
    ]
}

const PaymentList = (props)=>{
    return(        
        <div className="collapse show" id="collapseExample-2">
            <div className="table">
                <div className="table-options">
                    <button type="button" className="btn btn-refresh">Refresh</button>
                </div>
                <table className="table table-striped">
                    <thead>
                        <tr>
                            <th scope="col">Bank Code</th>
                            <th scope="col">Bank Name</th>
                            <th scope="col">Acc. NO.</th>
                            <th scope="col">Acc. Name</th>
                            <th scope="col">VIP Level</th>
                            <th scope="col" className="text-right">Min Amount</th>
                            <th scope="col" className="text-right">Max Amount</th>
                            <th scope="col" className="text-right">Daily Limit Amount</th>
                            <th scope="col">Purpose</th>
                            <th scope="col">Status</th>
                            <th scope="col" className="text-right">Dummy</th>
                            <th scope="col" className="text-right width-50">Sort</th>
                            <th scope="col" className="width-85">Action</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <th scope="row">AMB</th>
                            <td>AmBank Group</td>
                            <td>1887787878</td>
                            <td>Lee Ka Sheng</td>
                            <td>0</td>
                            <td className="text-right">10</td>
                            <td className="text-right">10,000</td>
                            <td className="text-right">500,000</td>
                            <td><span>Deposit</span></td>
                            <td><span className="status status-suspend">Suspend</span></td>
                            <td className="text-right">0</td>
                            <td className="text-right">0</td>
                            <td>
                                <div className="button-action">
                                    <button className="btn btn-edit">
                                        <i className="icon-edit"></i>
                                    </button>
                                    <button className="btn btn-icon-delete">
                                        <i className="icon-delete"></i>
                                    </button>
                                </div>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
    )
}

export default App;

if(process.env.NODE_ENV==='production'){
    ReactDOM.render(<App />, document.getElementById('content'))
}