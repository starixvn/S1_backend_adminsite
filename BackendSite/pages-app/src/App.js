if(process.env.NODE_ENV!=='production'){
  Promise.all([
    import('react-dom'),
    import('react'),
    import('./component/dropdownBox'),
    import('./UserDefine'),
  ]).then(([ReactDOM, React, elem, UserDefine])=>{
    const pages = UserDefine.pages;
    class PageHandler extends React.Component{
      constructor(props){
        super(props)
        const defaultPage = UserDefine.defaultPage;
        this.state = {func:defaultPage}
        this.RenderSelectedPage(defaultPage)
      }
      OnPageClick=(PageName)=>{
        this.setState({func:PageName}) 
        this.RenderSelectedPage(PageName)
      }
      RenderSelectedPage=(PageName)=>{
        ReactDOM.render(pages[PageName], document.getElementById('content'))
      }
      render(){
          let rows = Object.keys(pages).map(k=>{return k})
          return(
              <div className="panel panel--search">
                <div className="panel__row">
                  <elem.DropdownBox 
                    ColName="Function Page" 
                    Items={rows} 
                    OnSelectItem={this.OnPageClick} 
                    SelectValue={this.state.func}
                    />
                </div>
              </div>
          )
      }
    }
    
    ReactDOM.render(<PageHandler />, document.getElementById('PageHandler'));
  });
}